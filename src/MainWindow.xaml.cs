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

namespace ClashLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClashReports reports = new ClashReports();
        AvailableParameters availableParameters = new AvailableParameters();
        SelectedParameters selectedParameters = new SelectedParameters();

        public MainWindow()
        {
            InitializeComponent();
            //TODO
            //Sort parameters list alphabeticaly
            reportsListBox.DataContext = reports;
            LeftParamListBox.DataContext = availableParameters;
            RightParamListBox.DataContext = selectedParameters;
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            selectedParameters = (SelectedParameters)RightParamListBox.DataContext;
            reports = (ClashReports)reportsListBox.DataContext;

            if (selectedParameters.Count != 0)
            {
                reports.WriteToFile(selectedParameters);
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void addReport_Click(object sender, RoutedEventArgs e)
        {
            reports = (ClashReports)reportsListBox.DataContext;

            if (reports == null)
                return;

            reports.Add();

            availableParameters = (AvailableParameters)LeftParamListBox.DataContext;

            if (reports.Parameters.Count != 0)
            {
                foreach (Parameter param in reports.Parameters)
                {
                    if (!selectedParameters.Contains(param) && !availableParameters.Contains(param))
                    {
                        availableParameters.Add(param);
                    }
                }
            }

        }

        private void removeReport_Click(object sender, RoutedEventArgs e)
        {
            
            reports = (ClashReports)reportsListBox.DataContext;
            System.Collections.IList selectedReports = reportsListBox.SelectedItems;

            if (selectedReports.Count != 0)
            {
                reports.RemoveItems(selectedReports);
            }
        }

        private void addParam_Click(object sender, RoutedEventArgs e)
        {
            availableParameters = (AvailableParameters)LeftParamListBox.DataContext;
            selectedParameters = (SelectedParameters)RightParamListBox.DataContext;

            System.Collections.IList parametersSelection = LeftParamListBox.SelectedItems;


            if (parametersSelection.Count != 0)
            {
                //Add Param to the selected parameters list
                selectedParameters.AddItems(parametersSelection);
                //remove Param from the available parameters list
                availableParameters.RemoveItems(parametersSelection);
            }
        }

        private void removeParam_Click(object sender, RoutedEventArgs e)
        {
            availableParameters = (AvailableParameters)LeftParamListBox.DataContext;
            selectedParameters = (SelectedParameters)RightParamListBox.DataContext;

            System.Collections.IList parametersSelection = RightParamListBox.SelectedItems;

            if (parametersSelection.Count != 0)
            {
                //Add Param back to the available parameters list
                availableParameters.AddItems(parametersSelection);
                //remove Param from the selected parameters list
                selectedParameters.RemoveItems(parametersSelection);
            }
        }
    }
}
