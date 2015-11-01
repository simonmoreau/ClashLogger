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

        private string name;
        public string Name
        {
            get { return name; }
        }

        private DateTime creationDate;
        public DateTime CreationTime
        {
            get { return creationDate; }
        }

        private DateTime modificationDate;
        public DateTime ModificationDate
        {
            get { return modificationDate; }
        }

        private Navisworks.batchtest batchtest;
        public Navisworks.batchtest Batchtest
        {
            get { return batchtest; }
        }

        public ClashReport(FileInfo file)
        {
            fileInfo = file;
            name = file.Name;
            creationDate = file.CreationTime;
            modificationDate = file.LastWriteTime;
            batchtest = ReadReport(file.FullName);
            parameters = AddParameters(this);
        }

        public List<Parameter> parameters;
        private List<Parameter> Parameters
        {
            get { return parameters; }
        }

        private List<Parameter> AddParameters(ClashReport report)
        {
            List<Parameter> parameters = new List<Parameter>();

            Navisworks.clashtest clashtest = report.Batchtest.clashtests.FirstOrDefault();
            Navisworks.clashresult clashresult = clashtest.clashresults.FirstOrDefault(item => item.GetType() == typeof(clashresult)) as clashresult;
            Navisworks.clashgroup clashgroup = clashtest.clashresults.FirstOrDefault(item => item.GetType() == typeof(clashgroup)) as clashgroup;

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
            catch
            {
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
                    }
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


            foreach (ClashReport report in selectedReports)
            {
                this.Remove(report);
            }
        }

        private AvailableParameters availableParameters;
        public AvailableParameters AvailableParameters
        {
            get { return availableParameters; }
        }

    }
}
