using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Xml;


using XmlSeri = System.Xml.Serialization;
using System.Xml.Serialization;

namespace UWGame.SimSide
{
    public class CustomXmlSerializer
    {

        #region inner types

        public interface ISerializationProxy
        {
            object UnderlyingObject { get; set; }
            void Copy(object source, object dest);
            ISerializationProxy CreateInstance(object o);
        }

        public abstract class XmlTypeMappingBase
        {
            public abstract Type ProxiedType { get; }
            public abstract Type ProxyType { get; }
        }

        public class XmlTypeMapping<TProxied, TProxy> : XmlTypeMappingBase
        {
            public override Type ProxiedType { get { return typeof(TProxied); } }
            public override Type ProxyType { get { return typeof(TProxy); } }

            public Func<TProxy, TProxied> SetterMethod { get; set; }
            public Func<TProxied, TProxy> GetterMethod { get; set; }
        }

        public class XmlProxyData
        {
            public Type ProxiedType { get; set; }
            public List<XmlTypeMappingBase> TypeMappings { get; set; }

            public XmlProxyData(Type proxiedType) { this.ProxiedType = proxiedType; TypeMappings = new List<XmlTypeMappingBase>(); }
        }

        private class TypeData
        {
            public int Index { get; set; }
            public XmlTypeMappingBase TypeMapping { get; set; }
        }

        private class MemberData
        {
            public bool IsField { get; set; }
            public Type MemberType { get; set; }
            public string Name { get; set; }
            public object[] CustomAttributes { get; set; }
        }

        private class ProxyAssemblyData
        {
            public XmlProxyData XmlProxyData { get; set; }
            public Type ProxyType { get; set; }
            public Assembly ProxyAssembly { get; set; }
            public ISerializationProxy ProxyInstance { get; set; }
        }

        #endregion

        #region constants

        private const string UNDERLYING_INSTANCE_NAME = "_underlyingInstance";
        private const string PROXY_DATA_NAME = "_proxyData";

        #endregion

        #region variables

        private CodeCompileUnit _unit;
        private CodeNamespace _ns;
        private XmlProxyData _proxyData;
        private CodeTypeDeclaration _cl;
        private Dictionary<Type, TypeData> _typeDataMap;
        private List<MemberData> _memberList;

        #endregion

        #region static variables

        private static Dictionary<Type, ProxyAssemblyData> _proxyAssemblyDataMap = new Dictionary<Type, ProxyAssemblyData>();
        private static Dictionary<Type, object> _xmlCustomAttributesInstances = new Dictionary<Type, object>();

        #endregion

        #region public methods

        public CodeCompileUnit GenerateXmlProxy(XmlProxyData proxyData)
        {
            this._proxyData = proxyData;

            // creates a compile unit, namespace and the proxy class

            this._unit = new CodeCompileUnit();
            this._ns = new CodeNamespace("dummy.d" + Guid.NewGuid().ToString().Replace("-", ""));
            this._unit.Namespaces.Add(this._ns);

            this._cl = new CodeTypeDeclaration(proxyData.ProxiedType.Name);
            this._cl.IsClass = true;
            this._cl.Attributes = MemberAttributes.Public;
            this._cl.BaseTypes.Add(typeof(ISerializationProxy));
            this._ns.Types.Add(_cl);
            this.GenerateMembers();

            return this._unit;
        }

        public Assembly CompileAssembly(XmlProxyData proxyData)
        {
            string proxyTypeName;
            return CompileAssembly(proxyData, out proxyTypeName);
        }

        public Assembly CompileAssembly(XmlProxyData proxyData, out string proxyTypeName)
        {
            // compiles a dynamic assembly in memory from the generated compile unit
            // returns the name of the proxy class type

            CodeCompileUnit u = this.GenerateXmlProxy(proxyData);
            proxyTypeName = this._ns.Name + "." + this._cl.Name;

            CodeDomProvider provider = CodeDomProvider.CreateProvider("c#");

            // input params for the compiler
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.GenerateInMemory = true;

            // get all references from the entry assembly
            Assembly executingAssembly = Assembly.GetEntryAssembly();
            compilerParams.ReferencedAssemblies.Add(executingAssembly.Location);
            foreach (AssemblyName assemblyName in executingAssembly.GetReferencedAssemblies())
            {
                compilerParams.ReferencedAssemblies.Add(Assembly.Load(assemblyName).Location);
            }

            // generate the DLL
            compilerParams.GenerateExecutable = false;

            CompilerResults results = provider.CompileAssemblyFromDom(compilerParams, u);
            if (results.Errors.Count > 0) throw new ApplicationException("Compilation erros: " + results.Errors[0].ErrorText);
            
            return results.CompiledAssembly;
        }

        public string GetProxyCode(XmlProxyData proxyData) { return this.GenerateCode(this.GenerateXmlProxy(proxyData)); }
        public void SaveProxyCodeToFile(XmlProxyData proxyData, string filePath) { this.GenerateXmlProxy(proxyData); this.SaveFile(filePath); }

        #endregion

        #region static methods

        public static XmlProxyData GetXmlProxyData(Type proxiedType)
        {
            // used by the proxy classes to get the XmlProxyData for their corresponding proxied class

            if (_proxyAssemblyDataMap.ContainsKey(proxiedType)) return _proxyAssemblyDataMap[proxiedType].XmlProxyData;
            return null;
        }

        public static void WriteXmlSerialize(object o, XmlWriter writer, XmlProxyData proxyData)
        {
            if (o == null || writer == null || proxyData == null) throw new NullReferenceException();
            
            // check if the dynamic assembly of the proxy class is already generated
            Type proxiedType = proxyData.ProxiedType;
            if (!_proxyAssemblyDataMap.ContainsKey(proxiedType)) { GenerateProxyAssembly(proxyData); }

            ProxyAssemblyData assemblyData = _proxyAssemblyDataMap[proxiedType];

            XmlSeri.XmlSerializer ser = new XmlSeri.XmlSerializer(assemblyData.ProxyType);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");  

            // the drill is simple - the System.Xml.XmlSerializer outputs the root element before it calls IXmlSerializable.WriteXml
            // it also closes the element after IXmlSerializable.WriteXml - so we need to output only the stuff inside the root element
            // not the best way to do that though - some copying between XmlReader and XmlWriter
            object proxy = assemblyData.ProxyInstance.CreateInstance(o);
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (XmlTextWriter xw = new XmlTextWriter(sw))
                {
                   // ser.Serialize(xw, proxy);
                    ser.Serialize(xw, proxy, ns); // <-- Added namespace to get rid of xmlsns stuff...
                }
            }
            using (StringReader sr = new StringReader(sb.ToString()))
            {
                using (XmlTextReader xr = new XmlTextReader(sr))
                {
                    while (xr.Read()) { if (xr.NodeType == XmlNodeType.Element) break; }
                    WriteDeepNode(xr, writer, true);
                }
            }
        }

        public static void ReadXmlDeserialize(object o, XmlReader reader, XmlProxyData proxyData)
        {
            if (o == null || reader == null || proxyData == null) throw new NullReferenceException();
            Type proxiedType = proxyData.ProxiedType;
            if (!_proxyAssemblyDataMap.ContainsKey(proxiedType)) { GenerateProxyAssembly(proxyData); }

            ProxyAssemblyData assemblyData = _proxyAssemblyDataMap[proxiedType];

            if (reader.LocalName == assemblyData.ProxyType.Name)
            {
                XmlSeri.XmlSerializer ser = new XmlSeri.XmlSerializer(assemblyData.ProxyType);
                ISerializationProxy p = (ISerializationProxy)ser.Deserialize(reader);
                p.Copy(p.UnderlyingObject, o);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    using (XmlTextWriter xw = new XmlTextWriter(sw))
                    {
                        xw.WriteStartElement(assemblyData.ProxyType.Name);
                        bool hasContent = reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement;
                        WriteDeepNode(reader, xw, true);

                        if (hasContent) xw.WriteFullEndElement();
                        else xw.WriteEndElement();

                        // read to the next node
                        reader.Read();
                    }
                }
                using (StringReader sr = new StringReader(sb.ToString()))
                {
                    using (XmlTextReader xr = new XmlTextReader(sr))
                    {
                        XmlSeri.XmlSerializer ser = new XmlSeri.XmlSerializer(assemblyData.ProxyType);
                        ISerializationProxy p = (ISerializationProxy)ser.Deserialize(xr);
                        p.Copy(p.UnderlyingObject, o);
                    }
                }
            }
        }

        private static void GenerateProxyAssembly (XmlProxyData proxyData)
        {
            ProxyAssemblyData assemblyData = new ProxyAssemblyData() { XmlProxyData = proxyData  };
            _proxyAssemblyDataMap[proxyData.ProxiedType] = assemblyData;

            CustomXmlSerializer ser = new CustomXmlSerializer();
            string proxyTypeName;
            Assembly assembly = ser.CompileAssembly(proxyData, out proxyTypeName);
            assemblyData.ProxyAssembly = assembly;
            assemblyData.ProxyType = assembly.GetType(proxyTypeName);

            ConstructorInfo ci = assemblyData.ProxyType.GetConstructor(new Type[] { });
            assemblyData.ProxyInstance = (ISerializationProxy)ci.Invoke(new object[] { });
        }

        private static void WriteDeepNode(XmlReader reader, XmlWriter writer, bool skipStartElement)
        {
            int depth = 1;
            string startElementName = reader.LocalName;
            WriteShallowNode(reader, writer, skipStartElement);
            if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement) return;

            bool shouldBreak = false;
            while (reader.Read())
            {
                if (reader.LocalName == startElementName)
                {
                    if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement) depth++;
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        depth--;
                        if (depth == 0)
                        {
                            if (skipStartElement) return;
                            shouldBreak = true;
                        }
                    }
                }
                WriteShallowNode(reader, writer, false);
                if (shouldBreak) break;
            }
        }

        /// <summary>
        /// this method was taken from here http://blogs.msdn.com/mfussell/archive/2005/02/12/371546.aspx
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        private static void WriteShallowNode(XmlReader reader, XmlWriter writer, bool skipStartElement)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (!skipStartElement) writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    writer.WriteAttributes(reader, true);
                    if (!skipStartElement && reader.IsEmptyElement)
                    {
                        writer.WriteEndElement();
                    }
                    break;
                case XmlNodeType.Text: writer.WriteString(reader.Value); break;
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace: writer.WriteWhitespace(reader.Value); break;
                case XmlNodeType.CDATA: writer.WriteCData(reader.Value); break;
                case XmlNodeType.EntityReference: writer.WriteEntityRef(reader.Name); break;
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.ProcessingInstruction: writer.WriteProcessingInstruction(reader.Name, reader.Value); break;
                case XmlNodeType.DocumentType: writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value); break;
                case XmlNodeType.Comment: writer.WriteComment(reader.Value); break;
                case XmlNodeType.EndElement: writer.WriteFullEndElement(); break;
            }

        }

        #endregion

        #region private methods

        private void GenerateMembers()
        {
            this.InitTypeAndFieldData();
            this.CreateConstructorAndFields();
            this.CreateProxyInterfaceMembers();
            this.CreateProperties();
        }

        private void InitTypeAndFieldData()
        {
            int index = 0;
            this._typeDataMap = new Dictionary<Type, TypeData>();
            foreach (XmlTypeMappingBase mapping in this._proxyData.TypeMappings)
            {
                this._typeDataMap[mapping.ProxiedType] = new TypeData() { Index = index++, TypeMapping = mapping };
            }

            this._memberList = new List<MemberData>();
            this._memberList.AddRange(
            this._proxyData.ProxiedType.GetFields(BindingFlags.Instance | BindingFlags.Public).
                Select(f => new MemberData() { IsField = true, MemberType = f.FieldType, Name = f.Name, CustomAttributes = f.GetCustomAttributes(true) }));

            this._memberList.AddRange(
            this._proxyData.ProxiedType.GetProperties(BindingFlags.Instance | BindingFlags.Public).
                Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null).
                Select(p => new MemberData() { IsField = false, MemberType = p.PropertyType, Name = p.Name, CustomAttributes = p.GetCustomAttributes(true) }));
        }

        private void CreateProperties()
        {
            foreach (MemberData data in this._memberList)
            {
                CodeMemberProperty p = this.AddProperty(data.MemberType, data.Name);
                if (this._typeDataMap.ContainsKey(data.MemberType))
                {
                    TypeData typeData = this._typeDataMap[data.MemberType];
                    p.Type = new CodeTypeReference (typeData.TypeMapping.ProxyType);

                    CodeExpression exp = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(this._cl.Name), PROXY_DATA_NAME);
                    exp = new CodeFieldReferenceExpression(exp, "TypeMappings");
                    exp = new CodeIndexerExpression(exp, new CodePrimitiveExpression(typeData.Index));
                    exp = new CodeCastExpression(typeData.TypeMapping.GetType(), exp);

                    p.GetStatements.Add(
                            new CodeMethodReturnStatement(
                                    new CodeMethodInvokeExpression (
                                            new CodeMethodReferenceExpression (exp, "GetterMethod"),
                                            new CodeFieldReferenceExpression(this.GetThisField(UNDERLYING_INSTANCE_NAME), data.Name)
                                        )
                                )
                        );

                    p.SetStatements.Add(
                            new CodeAssignStatement(
                                    new CodeFieldReferenceExpression(this.GetThisField(UNDERLYING_INSTANCE_NAME), data.Name),
                                    new CodeMethodInvokeExpression(
                                            new CodeMethodReferenceExpression(exp, "SetterMethod"),
                                            new CodePropertySetValueReferenceExpression ()
                                        )
                                )
                        );
                }
                else
                {
                    p.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(this.GetThisField(UNDERLYING_INSTANCE_NAME), data.Name)));
                    p.SetStatements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(this.GetThisField(UNDERLYING_INSTANCE_NAME), data.Name),
                        new CodePropertySetValueReferenceExpression()));
                }

                AddCustomAttributes(p, data);
            }
        }

        private void AddCustomAttributes(CodeMemberProperty p, MemberData data)
        { 
            if (data.CustomAttributes == null) return;
            foreach (object attr in data.CustomAttributes)
            {
                Type attrType = attr.GetType();
                if (!attrType.Name.StartsWith("Xml")) continue;

                CodeAttributeDeclaration attrDecl = new CodeAttributeDeclaration(new CodeTypeReference(attrType));
                
                foreach (PropertyInfo pi in attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (pi.DeclaringType == attrType && (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string) || pi.PropertyType == typeof(Type)))
                    {
                        object val = pi.GetValue(attr, null);
                        if (!IsCustomAttributeDefaultValue(attrType, pi, val))
                            attrDecl.Arguments.Add(new CodeAttributeArgument(pi.Name, pi.PropertyType == typeof(Type) && val != null ? (CodeExpression)new CodeTypeOfExpression((Type)val) : (CodeExpression)new CodePrimitiveExpression(val)));
                    }
                }

                p.CustomAttributes.Add (attrDecl);
            }
        }

        private bool IsCustomAttributeDefaultValue(Type attrType, PropertyInfo pi, object value)
        {
            if (!_xmlCustomAttributesInstances.ContainsKey(attrType))
            {
                ConstructorInfo ci = attrType.GetConstructor(new Type[] { });
                if (ci == null) return false;
                _xmlCustomAttributesInstances[attrType] = ci.Invoke(new object[] { });
            }
            object defValue = pi.GetValue(_xmlCustomAttributesInstances[attrType], null);
            if (defValue == null) return value == null;
            return defValue.Equals(value);
        }

        private void CreateProxyInterfaceMembers()
        {
            CodeMemberProperty prop = this.AddProperty(typeof(object), "UnderlyingObject");
            prop.GetStatements.Add(new CodeMethodReturnStatement(this.GetThisField(UNDERLYING_INSTANCE_NAME)));
            prop.SetStatements.Add(
                    new CodeAssignStatement(
                        this.GetThisField(UNDERLYING_INSTANCE_NAME),
                        new CodeCastExpression(new CodeTypeReference(this._proxyData.ProxiedType), new CodePropertySetValueReferenceExpression())
                ));
            prop.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.Xml.Serialization.XmlIgnoreAttribute))));
                
            CodeMemberMethod m = this.AddMethod(typeof(ISerializationProxy), "CreateInstance");
            m.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "o"));
            m.Statements.Add(
                new CodeMethodReturnStatement (
                        new CodeObjectCreateExpression (new CodeTypeReference (this._cl.Name), GetVar ("o"))
                    )
                );

            m = this.AddMethod(typeof(void), "Copy");
            m.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "source"));
            m.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "dest"));
            m.Statements.Add(new CodeVariableDeclarationStatement (this._proxyData.ProxiedType, "s", 
                new CodeCastExpression(this._proxyData.ProxiedType, GetVar("source"))));
            m.Statements.Add(new CodeVariableDeclarationStatement(this._proxyData.ProxiedType, "d",
                new CodeCastExpression(this._proxyData.ProxiedType, GetVar("dest"))));

            foreach (MemberData data in this._memberList)
            {
                if (data.IsField)
                {
                    m.Statements.Add(new CodeAssignStatement(
                        GetFieldRef("d", data.Name), GetFieldRef("s", data.Name)));
                }
                else 
                {
                    m.Statements.Add(new CodeAssignStatement(
                        GetPropRef("d", data.Name), GetPropRef("s", data.Name)));
                }
            }
        }

        private void CreateConstructorAndFields()
        {
            this.AddField(this._cl, this._proxyData.ProxiedType, UNDERLYING_INSTANCE_NAME);
            CodeMemberField pData = this.AddField(this._cl, typeof(XmlProxyData), PROXY_DATA_NAME);

            pData.Attributes |= MemberAttributes.Static;
            pData.InitExpression = GetStaticMethodInvoke(GetSelfType(), "GetXmlProxyData",
                new CodeTypeOfExpression(new CodeTypeReference(this._proxyData.ProxiedType)));

            CodeConstructor c = new CodeConstructor();
            c.Attributes = MemberAttributes.Public;
            c.Statements.Add(new CodeAssignStatement(
                    GetThisField(UNDERLYING_INSTANCE_NAME), 
                    new CodeObjectCreateExpression( new CodeTypeReference(this._proxyData.ProxiedType))
                ));
            this._cl.Members.Add(c);

            c = new CodeConstructor();
            c.Attributes = MemberAttributes.Public;
            c.Parameters.Add (new CodeParameterDeclarationExpression(typeof(object),
                 UNDERLYING_INSTANCE_NAME));

            c.Statements.Add(new CodeAssignStatement(
                    GetThisField(UNDERLYING_INSTANCE_NAME),
                    new CodeCastExpression(this._proxyData.ProxiedType, GetVar(UNDERLYING_INSTANCE_NAME))));
            this._cl.Members.Add(c);
        }

        #endregion

        #region code helpers

        private CodeVariableReferenceExpression GetVar(string varName) { return new CodeVariableReferenceExpression(varName); }
        private CodeTypeReference GetSelfType() { return new CodeTypeReference(typeof(CustomXmlSerializer)); }
        private CodeExpression GetThis() { return new CodeThisReferenceExpression(); }

        private CodeExpression GetThisField(string fieldName) { return new CodeFieldReferenceExpression(GetThis(), fieldName); }

        private CodeMemberField AddField(CodeTypeDeclaration cl, Type type, string name)
        {
            CodeMemberField f = new CodeMemberField(type, name);
            f.Attributes = MemberAttributes.Private;
            cl.Members.Add(f); return f;
        }

        private CodeMethodInvokeExpression GetMethodInvoke(string varName, string methodName, params CodeExpression[] parameters)
        {
            return new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(varName), methodName),
                    parameters
                );
        }

        private CodeMethodInvokeExpression GetStaticMethodInvoke(CodeTypeReference type, string methodName, params CodeExpression[] parameters)
        {
            return new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(type), methodName),
                    parameters
                );
        }

        private CodeMemberProperty AddProperty(Type type, string name)
        {
            CodeMemberProperty p = new CodeMemberProperty();
            p.Name = name;
            p.Type = new CodeTypeReference(type);
            p.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            this._cl.Members.Add(p);
            return p;
        }

        private CodeMemberMethod AddMethod(Type returnType, string name)
        {
            CodeMemberMethod m = new CodeMemberMethod() { Name = name, ReturnType = new CodeTypeReference(returnType), Attributes = MemberAttributes.Public | MemberAttributes.Final };
            this._cl.Members.Add(m);
            return m;
        }

        private CodeFieldReferenceExpression GetFieldRef(string varName, string fieldName)
        {
            return new CodeFieldReferenceExpression(GetVar(varName), fieldName);
        }

        private CodePropertyReferenceExpression GetPropRef(string varName, string propName)
        {
            return new CodePropertyReferenceExpression(GetVar(varName), propName);
        }

        private CodeExpression GetFieldOrPropRef(string varName, string name, bool isField)
        {
            if (isField) return GetFieldRef(varName, name);
            return GetPropRef(varName, name);
        }

        #endregion

        #region generate code

        private void SaveFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string s = GenerateCode(this._unit);
                    sw.Write(s);
                    sw.Flush();
                }
            }
        }

        private string GenerateCode(CodeCompileUnit unit)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (IndentedTextWriter writer = new IndentedTextWriter(sw))
                {
                    CodeGeneratorOptions options = new CodeGeneratorOptions();
                    options.BlankLinesBetweenMembers = true;
                    options.ElseOnClosing = false;
                    options.VerbatimOrder = false;
                    provider.GenerateCodeFromCompileUnit(unit, writer, options);
                }
            }
            return sb.ToString();
        }

        #endregion
    }

}

