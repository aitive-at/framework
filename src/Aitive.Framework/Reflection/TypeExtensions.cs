namespace Aitive.Framework.Reflection;

public static class TypeExtensions
{
    extension(Type type)
    {
        public bool IsDefaultConstructibleClass
        {
            get
            {
                return type is { IsClass: true, IsAbstract: false }
                    && type.GetConstructor(Type.EmptyTypes) is not null;
            }
        }
    }
}
