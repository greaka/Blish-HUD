using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Glide
{
    internal class MemberAccessor
    {
        private static readonly BindingFlags flags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        protected Func<object, object> getMethod;
        protected Action<object, object> setMethod;

        public MemberAccessor(object target, string name, bool writeRequired = true)
        {
            var T = target.GetType();
            PropertyInfo propInfo = null;
            FieldInfo fieldInfo = null;
            this.Target = target;

            if ((propInfo = T.GetProperty(name, flags)) != null)
            {
                this.MemberType = propInfo.PropertyType;
                this.MemberName = propInfo.Name;

                {
                    var param = Expression.Parameter(typeof(object));
                    var instance = Expression.Convert(param, propInfo.DeclaringType);
                    var convert = Expression.TypeAs(Expression.Property(instance, propInfo), typeof(object));
                    this.getMethod = Expression.Lambda<Func<object, object>>(convert, param).Compile();
                }

                if (writeRequired)
                {
                    var param = Expression.Parameter(typeof(object));
                    var argument = Expression.Parameter(typeof(object));
                    var setterCall = Expression.Call(
                        Expression.Convert(param, propInfo.DeclaringType),
                        propInfo.GetSetMethod(),
                        Expression.Convert(argument, propInfo.PropertyType));

                    this.setMethod = Expression.Lambda<Action<object, object>>(setterCall, param, argument).Compile();
                }
            }
            else if ((fieldInfo = T.GetField(name, flags)) != null)
            {
                this.MemberType = fieldInfo.FieldType;
                this.MemberName = fieldInfo.Name;

                {
                    var self = Expression.Parameter(typeof(object));
                    var instance = Expression.Convert(self, fieldInfo.DeclaringType);
                    var field = Expression.Field(instance, fieldInfo);
                    var convert = Expression.TypeAs(field, typeof(object));
                    this.getMethod = Expression.Lambda<Func<object, object>>(convert, self).Compile();
                }

                {
                    var self = Expression.Parameter(typeof(object));
                    var value = Expression.Parameter(typeof(object));

                    var fieldExp = Expression.Field(Expression.Convert(self, fieldInfo.DeclaringType), fieldInfo);
                    var assignExp = Expression.Assign(fieldExp, Expression.Convert(value, fieldInfo.FieldType));

                    this.setMethod = Expression.Lambda<Action<object, object>>(assignExp, self, value).Compile();
                }
            }
            else
            {
                throw new Exception(string.Format("Field or {0} property '{1}' not found on object of type {2}.",
                    writeRequired ? "read/write" : "readable",
                    name, T.FullName));
            }
        }

        public object Target { get; }
        public string MemberName { get; }
        public Type MemberType { get; }

        public object Value
        {
            get => this.getMethod(this.Target);
            set => this.setMethod(this.Target, value);
        }
    }
}