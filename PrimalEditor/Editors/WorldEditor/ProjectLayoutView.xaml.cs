using PrimalEditor.Components;
using PrimalEditor.Editors;
using PrimalEditor.GameProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// ProjectLayoutView.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectLayoutView : UserControl
    {
        public ProjectLayoutView()
        {
            InitializeComponent();
        }

        private void OnAddGameEntity_Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var vm = btn.DataContext as Scene;
            vm.AddGameEntityCommand.Execute(new GameEntity(vm) { Name = "Empty Game Entity" });
        }

        private void OnGameEntities_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var entity = (sender as ListBox).SelectedItems[0];
            GameEntityView.Instance.DataContext = entity;
        }

        // 现在替换掉 OnAddScene_Button_Click 函数 成 视图模型的绑定事件处理器 => Command="{Binding AddScene}"
        //private void OnAddScene_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    var vm = DataContext as Project;
        //    vm.AddScene("New Scene " + vm.Scenes.Count);
        //}
    }
}
