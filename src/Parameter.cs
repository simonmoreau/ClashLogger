using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;

namespace ClashLogger
{
    public class Parameter
    {
        private string name;
        public string Name
        {
            get { return name; }
        }

        public Parameter(string _name)
        {
            name = _name;
        }
    }

    public class Parameters : ObservableCollection<Parameter>
    {
        public void RemoveItems(System.Collections.IList selectedParametersIList)
        {
            List<Parameter> selectedParameters = new List<Parameter>();

            foreach (Parameter parameter in selectedParametersIList)
            {
                if (this.Contains(parameter))
                {
                    selectedParameters.Add(parameter);
                }
            }

            foreach (Parameter parameter in selectedParameters)
            {
                this.Remove(parameter);
            }
        }

        public void AddItems(System.Collections.IList selectedParametersIList)
        {
            foreach (Parameter parameter in selectedParametersIList)
            {
                if (!this.Contains(parameter))
                {
                    this.Add(parameter);
                }
            }
        }
    }

    public class AvailableParameters : Parameters
    {
        public AvailableParameters()
        {
            this.Add(new Parameter("Param1"));
            this.Add(new Parameter("Param2"));
            this.Add(new Parameter("Param3"));
            this.Add(new Parameter("Param4"));
            this.Add(new Parameter("Param5"));
            this.Add(new Parameter("Param6"));
        }
    }

    public class SelectedParameters : Parameters
    {
        public SelectedParameters()
        {

        }
    }

}
