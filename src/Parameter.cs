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

        private Type associatedType;
        public Type AssociatedType
        {
            get { return associatedType; }
        }

        public Parameter(string _name, Type _associatedType)
        {
            name = _name;
            associatedType = _associatedType;
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

        }
    }

    public class SelectedParameters : Parameters
    {
        public SelectedParameters()
        {

        }
    }

}
