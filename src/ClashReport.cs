using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using ClashLogger.Navisworks;
using System.Reflection;

namespace ClashLogger
{
    public class ClashReport
    {
        private FileInfo fileInfo;
        public FileInfo FileInfo
        {
            get { return fileInfo; }
            set { fileInfo = value; }
        }

        private string reportName;
        public string ReportName
        {
            get { return reportName; }
        }

        private DateTime reportCreationDate;
        public DateTime ReportCreationTime
        {
            get { return reportCreationDate; }
        }

        private DateTime reportModificationDate;
        public DateTime ReportModificationDate
        {
            get { return reportModificationDate; }
        }

        private Navisworks.batchtest batchtest;
        public Navisworks.batchtest Batchtest
        {
            get { return batchtest; }
        }

        public ClashReport(FileInfo file)
        {
            fileInfo = file;
            reportName = file.Name;
            reportCreationDate = file.CreationTime;
            reportModificationDate = file.LastWriteTime;
            batchtest = ReadReport(file.FullName);
            if (batchtest != null)
            {
                parameters = AddParameters(this);
            }
        }

        private List<Parameter> parameters;
        public List<Parameter> Parameters
        {
            get { return parameters; }
        }

        private List<Parameter> AddParameters(ClashReport report)
        {
            List<Parameter> parameters = new List<Parameter>();

            Navisworks.clashtest clashtest = report.Batchtest.clashtests.FirstOrDefault();
            Navisworks.clashresult clashresult = clashtest.clashresults.FirstOrDefault(item => item.GetType() == typeof(clashresult)) as clashresult;
            Navisworks.clashgroup clashgroup = clashtest.clashresults.FirstOrDefault(item => item.GetType() == typeof(clashgroup)) as clashgroup;

            parameters.AddRange(loadParameters(report));
            parameters.AddRange(loadParameters(clashtest));
            parameters.AddRange(loadParameters(clashresult));
            parameters.AddRange(loadParameters(clashgroup));

            return parameters;
        }

        private List<Parameter> loadParameters(object obj)
        {
            List<Parameter> parameters = new List<Parameter>();

            if (obj != null)
            {
                Type objectType = obj.GetType();

                foreach (PropertyInfo propertyInfo in objectType.GetProperties())
                {
                    //Add only parameters of a specific type (string, dateandtime, double, int, ...)
                    if (propertyInfo.PropertyType == typeof(int)
                        || propertyInfo.PropertyType == typeof(string)
                        || propertyInfo.PropertyType == typeof(DateTime)
                        || propertyInfo.PropertyType == typeof(double)
                        || propertyInfo.PropertyType == typeof(float)
                        || propertyInfo.PropertyType == typeof(bool))
                    {
                        //add the property if it come from the xml
                        object[] attribs = propertyInfo.GetCustomAttributes(typeof(XmlIgnoreAttribute), false);
                        if (attribs.Length == 0)
                        {
                            //If the property is already added to the availableParameters, do not add
                            if (!parameters.Any(item => item.Name == propertyInfo.Name))
                            {
                                //Check if the value is not null
                                if (propertyInfo.GetValue(obj) != null)
                                {
                                    parameters.Add(new Parameter(propertyInfo.Name));
                                }
                            }
                        }
                    }
                }
            }

            return parameters;
        }

        private Navisworks.batchtest ReadReport(string fileName)
        {
            Navisworks.exchange exchange = new Navisworks.exchange();
            try
            {
                using (Stream objStream = new FileStream(fileName, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Navisworks.exchange));
                    StreamReader sr = new StreamReader(objStream);

                    //TODO
                    //display a warning when the selected file is not a Navisworks clash report
                    using (XmlReader reader = XmlReader.Create(sr))
                    {
                        exchange = (Navisworks.exchange)serializer.Deserialize(reader);
                    }
                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(fileName + ex.Message);
                return null;
            }

            Navisworks.batchtest returnValue = exchange.Items.First() as Navisworks.batchtest;

            return returnValue;
        }
    }

    public class ClashReports : ObservableCollection<ClashReport>
    {
        public ClashReports()
        {
            parameters = new Parameters();
        }

        public void Add()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    if (!this.Any(item => item.FileInfo.FullName == filename))
                    {
                        ClashReport newClashReport = new ClashReport(new FileInfo(filename));
                        this.Add(newClashReport);
                        if (newClashReport.Batchtest != null)
                        {
                            this.AddParameters(newClashReport);
                        }
                    }
                }
            }
        }

        private void AddParameters(ClashReport newClashReport)
        {

            foreach (Parameter param in newClashReport.Parameters)
            {
                if (!parameters.Any(item => item.Name == param.Name))
                {
                    parameters.Add(param);
                }
            }
        }

        public void RemoveItems(System.Collections.IList selectedReportsIList)
        {
            List<ClashReport> selectedReports = new List<ClashReport>();

            foreach (ClashReport report in selectedReportsIList)
            {
                if (this.Contains(report))
                {
                    selectedReports.Add(report);
                }
            }
            //TODO
            //Remove unused parameters
            foreach (ClashReport report in selectedReports)
            {
                this.Remove(report);
            }
        }

        public void WriteToFile(System.Collections.IList selectedParameters)
        {
            List<string> lines = new List<string>();

            string firstRow = "";
            foreach (Parameter selectedParameter in selectedParameters)
            {
                firstRow = firstRow + ";" + selectedParameter.Name;
            }
            lines.Add(firstRow);

            foreach (ClashReport report in this)
            {
                if (report.Batchtest != null)
                {
                    foreach (Navisworks.clashtest clashtest in report.Batchtest.clashtests)
                    {
                        foreach (object obj in clashtest.clashresults)
                        {
                            object objGroup = new clashgroup();
                            object objResult = new clashresult();

                            if (obj.GetType() == typeof(clashgroup))
                            {
                                objGroup = obj;
                                objResult = null;
                            }
                            else
                            {
                                objResult = obj;
                                objGroup = null;
                            }

                            lines.Add(
                                GetValuesAsString(report, typeof(ClashReport), selectedParameters) +
                                GetValuesAsString(clashtest, typeof(clashtest), selectedParameters) +
                                GetValuesAsString(objGroup, typeof(clashgroup), selectedParameters) +
                                GetValuesAsString(objResult, typeof(clashresult), selectedParameters)
                                );
                        }
                    }
                }
            }

            string path = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveDialog.Filter = "Csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveDialog.AddExtension = true;
            saveDialog.DefaultExt = "csv";

            if (saveDialog.ShowDialog() == true)
            {
                path = saveDialog.FileName;
                File.WriteAllLines(path, lines.ToArray());
            }

        }

        private string GetValuesAsString(object obj, Type type, System.Collections.IList selectedParameters)
        {
            string returnValue = "";
            PropertyInfo[] propertiesInfo = type.GetProperties();

            if (obj != null)
            {
                foreach (Parameter selectedParameter in selectedParameters)
                {
                    if (propertiesInfo.Any(item => item.Name == selectedParameter.Name))
                    {
                        PropertyInfo propertyInfo = type.GetProperty(selectedParameter.Name);

                        //Check if the value is not null
                        if (propertyInfo.GetValue(obj) != null)
                        {
                            returnValue = returnValue + ";" + propertyInfo.GetValue(obj).ToString();
                        }
                        else
                        {
                            returnValue = returnValue + ";";
                        }
                    }
                }
            }
            else
            {
                foreach (Parameter selectedParameter in selectedParameters)
                {
                    if (propertiesInfo.Any(item => item.Name == selectedParameter.Name))
                    {
                        returnValue = returnValue + ";";
                    }
                }
            }

            return returnValue;
        }

        private Parameters parameters;
        public Parameters Parameters
        {
            get { return parameters; }
        }

    }
}
