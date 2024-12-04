using System.Collections.Generic;

namespace ThesisApplication
{
    public class RequestVariable
    {
        public string name;
        public string value;

        public RequestVariable(string _name, string _value)
        {
            name = _name;
            value = _value;
        }

        public static string Join(RequestVariable[] variables)
        {
            string variablesConcat = "";
            foreach (RequestVariable n in variables) variablesConcat = $"{variablesConcat}&{n.name}={n.value}";
            return variablesConcat.Trim('&');
        }

        public static RequestVariable[] FromArray(string[] keys, string[] values)
        {
            List<RequestVariable> variables = new List<RequestVariable>();
            for (int i = 0; i < keys.Length; i++) variables.Add(new RequestVariable(keys[i], values[i]));
            return variables.ToArray();
        }

        public static RequestVariable[] FromSingle(string keys, string values)
        {
            return new RequestVariable[] { new RequestVariable(keys, values) };
        }
    }
}