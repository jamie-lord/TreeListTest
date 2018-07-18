using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace TreeListTest
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            TreeListView.ItemsSource = ItemsSource;
        }

        public TreeDataSource ItemsSource = new TreeDataSource(RootNode());

        private static Node RootNode()
        {
            return new Node(0, $"Root node");
        }
    }

    public class TreeDataSource : Tree, ISupportIncrementalLoading
    {
        public TreeDataSource(Node rootNode)
            : base(rootNode)
        {
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return LoadMoreItems().AsAsyncOperation();
        }

        public async Task<LoadMoreItemsResult> LoadMoreItems()
        {
            var count = await LoadMoreNodes();

            if (count == 0)
            {
                HasMoreItems = false;
            }

            return new LoadMoreItemsResult() { Count = (uint)count };
        }

        public bool HasMoreItems { get; private set; } = true;
    }

    public abstract class Tree : ObservableCollection<Node>
    {
        private const int PAGE_SIZE = 10;
        private readonly Node _rootNode;

        public Tree(Node rootNode)
        {
            _rootNode = rootNode;
            _rootNode.ChildTree.CollectionChanged += ChildTree_CollectionChanged;
        }

        private void ChildTree_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var nodeIndex = e.NewStartingIndex + i;
                    Insert(nodeIndex, (Node)e.NewItems[i]);
                }
            }
        }

        protected async Task<int> LoadMoreNodes()
        {
            return await _rootNode.LoadChildren(PAGE_SIZE);
        }
    }

    public class Node
    {
        private const int MAX_DEPTH = 10;

        public string Label { get; }

        public static Random R = new Random();

        public Thickness Padding
        {
            get
            {
                return new Thickness(_depth * 10, 0, 0, 0);
            }
        }

        public ObservableCollection<Node> ChildTree { get; } = new ObservableCollection<Node>();

        public Node(int depth, string name, int children = 2)
        {
            Label = name;
            _count = R.Next(1, 3);
            _depth = depth;
        }

        public readonly int _depth;

        private readonly int _count;

        private IList<Node> _children = new List<Node>();

        public async Task<int> LoadChildren(int minToLoad)
        {
            int haveLoaded = 0;
            foreach (var child in _children)
            {
                var c = await child.LoadChildren(minToLoad);
                minToLoad -= c;
                haveLoaded += c;

                if (minToLoad <= 0)
                {
                    break;
                }
            }

            while (minToLoad > 0)
            {
                string item = await LoadNextItem();

                if (item != null)
                {
                    Node node = new Node(_depth + 1, item);
                    ChildTree.Add(node);
                    _children.Add(node);
                    node.ChildTree.CollectionChanged += (s, e) => ChildTree_CollectionChanged(node, e);
                    minToLoad -= 1;
                    haveLoaded += 1;
                    if (minToLoad > 0)
                    {
                        var c = await node.LoadChildren(minToLoad);
                        minToLoad -= c;
                        haveLoaded += c;
                    }
                    if (minToLoad <= 0)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return haveLoaded;
        }

        private async Task<string> LoadNextItem()
        {
            await LoadItems();

            if (_items.Count > 0)
            {
                return _items.Dequeue();
            }

            return null;
        }

        private Queue<string> _items;

        private async Task LoadItems()
        {
            if (_items == null)
            {
                _items = new Queue<string>();

                if (_depth < MAX_DEPTH)
                {
                    await Task.Delay(500);
                    for (int i = 0; i < _count; i++)
                    {
                        _items.Enqueue($"{Label}, Child {i}");
                    }
                }
            }
        }

        private void ChildTree_CollectionChanged(Node sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int startIndex = ChildTree.IndexOf(sender);

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var nodeIndex = startIndex + 1 + e.NewStartingIndex + i;
                    ChildTree.Insert(nodeIndex, (Node)e.NewItems[i]);
                }
            }
        }
    }
}
