
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class SkipEmptyCollectionsContractResolver : DefaultContractResolver
    {
        public static readonly SkipEmptyCollectionsContractResolver Instance = new SkipEmptyCollectionsContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property =  base.CreateProperty(member, memberSerialization);

            if (property.PropertyType != typeof(string))
            {
                if (property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
                {
                    property.ShouldSerialize = instance => (instance?.GetType().GetProperty(property.PropertyName).GetValue(instance) as IEnumerable<object>)?.Count() > 0;
                }
            }
            return property;
        }
    }
}