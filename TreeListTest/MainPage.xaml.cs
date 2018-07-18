using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TreeListTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            TreeListView.ItemsSource = ItemsSource;
        }

        public TreeDataSource ItemsSource = new TreeDataSource(RootNodes());

        private static IEnumerable<Node> RootNodes()
        {
            for (int i = 0; i < 100; i++)
            {
                yield return new Node(0, $"Root node {i}", i);
            }
        }
    }

    public class TreeDataSource : ObservableCollection<Node>, ISupportIncrementalLoading
    {
        private readonly IEnumerable<Node> _nodes;

        private bool _hasMoreItems = true;

        private int _page = 0;

        private const int PAGE_SIZE = 10;

        public TreeDataSource(IEnumerable<Node> rootNodes)
        {
            _nodes = rootNodes.FlattenWithLevel(X => X.GetChildren()).Select(x => x.Item1);
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var dispatcher = Window.Current.Dispatcher;

            return Task.Run(async () =>
            {
                uint resultCount = 0;

                IEnumerable<Node> page = _nodes.GetPage(_page, PAGE_SIZE);

                _page++;

                if (page == null || page.Count() == 0)
                {
                    _hasMoreItems = false;
                }
                else
                {
                    resultCount = (uint)page.Count();

                    await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var node in page)
                        {
                            Add(node);
                        }
                    });
                }

                return new LoadMoreItemsResult() { Count = resultCount };
            }).AsAsyncOperation();
        }

        public bool HasMoreItems => _hasMoreItems;
    }

    public static class NodeOperations
    {
        public static IEnumerable<Tuple<Node, int>> FlattenWithLevel(this IEnumerable<Node> items, Func<Node, IEnumerable<Node>> getChildren)
        {
            var stack = new Stack<Tuple<Node, int>>();
            foreach (var item in items.Reverse())
            {
                stack.Push(new Tuple<Node, int>(item, 1));
            }

            while (stack.Count > 0)
            {
                Tuple<Node, int> current = stack.Pop();
                yield return current;
                foreach (Node child in getChildren(current.Item1).Reverse())
                {
                    stack.Push(new Tuple<Node, int>(child, current.Item2 + 1));
                }
            }
        }

        public static IEnumerable<Node> GetPage(this IEnumerable<Node> input, int page, int pagesize)
        {
            return input.Skip(page * pagesize).Take(pagesize);
        }
    }

    public class Node
    {
        private const int MAX_DEPTH = 4;

        public string Label { get; }

        public Thickness Padding
        {
            get
            {
                return new Thickness(_depth * 10, 0, 0, 0);
            }
        }

        public Node(int depth, string name, int children = 2)
        {
            Label = name;
            _count = children;
            _depth = depth;
        }

        public readonly int _depth;
        private readonly int _count;

        public IEnumerable<Node> GetChildren()
        {
            if (_depth == MAX_DEPTH)
            {
                yield break;
            }
            for (int i = 0; i < _count; i++)
            {
                yield return new Node(_depth + 1, $"{Label}, Child {i}");
            }
        }
    }
}
