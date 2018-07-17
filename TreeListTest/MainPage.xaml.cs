using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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

            TreeListView.ItemsSource = Nodes;
        }

        public ObservableCollection<Node> Nodes = new ObservableCollection<Node>(RootNodes());

        private static IEnumerable<Node> RootNodes()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new Node($"Root node {i}");
            }
        }
    }

    public class Node
    {
        public string Label { get; }

        public Node(string name)
        {
            Label = name;
        }

        private static Random _random;

        private static int _randomNumber
        {
            get
            {
                if (_random == null)
                {
                    _random = new Random();
                }

                return _random.Next(0, 5);
            }
        }

        public IEnumerable<Node> GetChildren()
        {
            for (int i = 0; i < _randomNumber; i++)
            {
                yield return new Node($"Child {i} of {Label}");
            }
        }
    }
}
