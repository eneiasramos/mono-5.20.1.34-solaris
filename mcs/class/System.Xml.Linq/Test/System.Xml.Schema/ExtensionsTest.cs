//
// ExtensionsTest.cs - NUnit Test Cases for Extensions.cs class under 
// System.Xml.Schema namespace found in System.Xml.Linq assembly 
// (System.Xml.Linq.dll) 
//
// Author:
//      Stefan Prutianu (stefanprutianu@yahoo.com)
//
// (C) Stefan Prutianu
// 

#if !MOBILE

using NUnit.Framework;
using System;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Collections.Generic;
using ExtensionsClass = System.Xml.Schema.Extensions;

namespace MonoTests.System.Xml.Schema
{

	[TestFixture]
	public class ExtensionsTest {

		public static String xsdString;
		public static XmlSchemaSet schemaSet;
		public static String xmlString;
		public static XDocument xmlDocument;
		public static bool validationSucceded;

		// initialize values for tests 
		[SetUp]
		public void Init () 
		{
		
			xsdString = @"<?xml version='1.0'?>
				   <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
				     <xs:element name='note'>
				       <xs:complexType>
					 <xs:sequence>
					   <xs:element name='to' type='xs:string'/>     
					   <xs:element name='from' type='xs:string'/>
					   <xs:element name='heading' type='xs:string'/>
					   <xs:element name='ps' type='xs:string' 
						maxOccurs='1' minOccurs = '0' default='Bye!'/>     
					   <xs:element name='body'>
					      <xs:simpleType>
						 <xs:restriction base='xs:string'>
						    <xs:minLength value='5'/>
						    <xs:maxLength value='30'/>
						 </xs:restriction>
					      </xs:simpleType>
					   </xs:element>        
					 </xs:sequence>
					 <xs:attribute name='subject' type='xs:string'
					   default='mono'/> 
					 <xs:attribute name='date' type='xs:date'
					   use='required'/>            
				       </xs:complexType>
				     </xs:element>
				   </xs:schema>";
			schemaSet = new XmlSchemaSet ();
			schemaSet.Add ("", XmlReader.Create (new StringReader (xsdString)));

			xmlString = @"<?xml version='1.0'?>
				      <note date ='2010-05-26'>
					 <to>Tove</to>
					 <from>Jani</from>
					 <heading>Reminder</heading>
					 <body>Don't forget to call me!</body>
				      </note>";
			xmlDocument = XDocument.Load (new StringReader (xmlString));
			validationSucceded = false;
		

			/*
			 * Use this method to load the above data from disk
			 * Comment the above code when using this method.
			 * Also the arguments for the folowing tests: XAttributeSuccessValidate
			 * XAttributeFailValidate, XAttributeThrowExceptionValidate, XElementSuccessValidate
			 * XElementFailValidate,XElementThrowExceptionValidate, XAttributeGetSchemaInfo
			 * XElementGetSchemaInfo may need to be changed.
			 */
			//LoadOutsideDocuments ("c:\\note.xsd", "c:\\note.xml");
		}

		// Use this method to load data from disk
		public static void LoadOutsideDocuments (String xsdDocumentPath, String xmlDocumentPath)
		{
			// Create a resolver with default credentials.
			XmlUrlResolver resolver = new XmlUrlResolver ();
			resolver.Credentials = global::System.Net.CredentialCache.DefaultCredentials;
			// Set the reader settings object to use the resolver.
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.XmlResolver = resolver;

			// Create the XmlReader object.
			XmlReader reader = XmlReader.Create (xsdDocumentPath, settings);
	       
			schemaSet = new XmlSchemaSet ();
			schemaSet.Add ("", reader);

			reader = XmlReader.Create (xmlDocumentPath, settings);
			xmlDocument = XDocument.Load (reader);
			validationSucceded = false;

		}

		// this gets called when a validation error occurs 
		public void TestValidationHandler (object sender, ValidationEventArgs e)
		{
			validationSucceded = false;
		}

		[TearDown]
		public void Cleanup () 
		{
			xsdString = null;
			schemaSet = null;
			xmlString = null;
			xmlDocument = null;
			validationSucceded = false;
		}


		// test succesfull validation
		[Test]
		public void XDocumentSuccessValidate () 
		{       
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (true, validationSucceded, "XDocumentSuccessValidate");
		}

		// test failed validation
		[Test]
		public void XDocumentFailValidate ()
		{       
			String elementName = "AlteringElementName";
			object elementValue = "AlteringElementValue";

			// alter XML document to invalidate
			XElement newElement = new XElement (elementName, elementValue);
			xmlDocument.Root.Add (newElement);

			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (false, validationSucceded, "XDocumentFailValidate");
		}

		/* 
		 * test if exception is thrown when xml document does not
		 * conform to the xml schema
		 */
		[Test]
		[ExpectedException (typeof (XmlSchemaValidationException))]
		public void XDocumentThrowExceptionValidate ()
		{
			String elementName = "AlteringElementName";
			object elementValue = "AlteringElementValue";

			// alter XML document to invalidate
			XElement newElement = new XElement (elementName, elementValue);
			xmlDocument.Root.Add (newElement);

			ExtensionsClass.Validate (xmlDocument, schemaSet, null);
		}

		/* 
		 * test xml validation populating the XML tree with
		 * the post-schema-validation infoset (PSVI)
		 */
		[Test]
		public void XDocumentAddSchemaInfoValidate ()
		{      
			// no. of elements before validation
			IEnumerable<XElement> elements = xmlDocument.Elements ();
			IEnumerator<XElement> elementsEnumerator = elements.GetEnumerator ();
			int beforeNoOfElements = 0;
			int beforeNoOfAttributes = 0;
			while (elementsEnumerator.MoveNext ()) {
					beforeNoOfElements++;
					if (!elementsEnumerator.Current.HasAttributes)
						continue;
					else {
						IEnumerable<XAttribute> attributes = elementsEnumerator.Current.Attributes ();
						IEnumerator<XAttribute> attributesEnumerator = attributes.GetEnumerator ();
						while (attributesEnumerator.MoveNext ())
							beforeNoOfAttributes++;        
					}    
				}

			// populate XML with PSVI
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), true);
			Assert.AreEqual (true, validationSucceded);

			// no. of elements after validation
			elements = xmlDocument.Elements ();
			elementsEnumerator = elements.GetEnumerator ();
			int afterNoOfElements = 0;
			int afterNoOfAttributes = 0;
			while (elementsEnumerator.MoveNext ()) {
				afterNoOfElements++;
				if (!elementsEnumerator.Current.HasAttributes)
					continue;
				else {
					IEnumerable<XAttribute> attributes = elementsEnumerator.Current.Attributes ();
					IEnumerator<XAttribute> attributesEnumerator = attributes.GetEnumerator ();
					while (attributesEnumerator.MoveNext ())
						afterNoOfAttributes++;
				}
			}

			AssertHelper.GreaterOrEqual (afterNoOfAttributes, beforeNoOfAttributes, "newAttributes");
			AssertHelper.GreaterOrEqual (afterNoOfElements, beforeNoOfElements, "newElements");       
		}

		/*
		 * test xml validation without populating the XML tree with 
		 * the post-schema-validation infoset (PSVI).
		 */
		[Test]
		public void XDocumentNoSchemaInfoValidate ()
		{
			// no. of elements before validation
			IEnumerable<XElement> elements = xmlDocument.Elements ();
			IEnumerator<XElement> elementsEnumerator = elements.GetEnumerator ();
			int beforeNoOfElements = 0;
			int beforeNoOfAttributes = 0;
			while (elementsEnumerator.MoveNext ()) {
				beforeNoOfElements++;
				if (!elementsEnumerator.Current.HasAttributes)
					continue;
				else {
					IEnumerable<XAttribute> attributes = elementsEnumerator.Current.Attributes ();
					IEnumerator<XAttribute> attributesEnumerator = attributes.GetEnumerator ();
					while (attributesEnumerator.MoveNext ())
						beforeNoOfAttributes++;
				}
			}

			// don't populate XML with PSVI
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), false);
			Assert.AreEqual (true, validationSucceded);

			// no. of elements after validation
			elements = xmlDocument.Elements ();
			elementsEnumerator = elements.GetEnumerator ();
			int afterNoOfElements = 0;
			int afterNoOfAttributes = 0;
			while (elementsEnumerator.MoveNext ()) {
				afterNoOfElements++;
				if (!elementsEnumerator.Current.HasAttributes)
					continue;
				else {
					IEnumerable<XAttribute> attributes = elementsEnumerator.Current.Attributes ();
					IEnumerator<XAttribute> attributesEnumerator = attributes.GetEnumerator ();
					while (attributesEnumerator.MoveNext ())
						afterNoOfAttributes++;
				}
			}

			Assert.AreEqual (afterNoOfAttributes, beforeNoOfAttributes, "oldAttributes");
			Assert.AreEqual (afterNoOfElements, beforeNoOfElements, "oldElements"); 
		 
		}

		// attribute validation succeeds after change
		[Test]
		public void XAttributeSuccessValidate ()
		{
			String elementName = "note";
			String attributeName = "date";
			object attributeValue = "2010-05-27";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), true);
			Assert.AreEqual (true, validationSucceded);

			// change and re-validate attribute value
			XAttribute date = xmlDocument.Element (elementName).Attribute (attributeName);
			date.SetValue (attributeValue);
			ExtensionsClass.Validate (date, date.GetSchemaInfo ().SchemaAttribute,schemaSet, 
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (true, validationSucceded, "XAttributeSuccessValidate");
		}

		// attribute validation fails after change
		[Test]
		public void XAttributeFailValidate ()
		{
			String elementName = "note";
			String attributeName = "date";
			object attributeValue = "2010-12-32";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler),true);
			Assert.AreEqual (true, validationSucceded);

			// change and re-validate attribute value
			XAttribute date = xmlDocument.Element (elementName).Attribute (attributeName);
			date.SetValue (attributeValue);
			ExtensionsClass.Validate (date, date.GetSchemaInfo ().SchemaAttribute, schemaSet, 
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (false, validationSucceded, "XAttributeFailValidate");
		}

		/* 
		 * test if exception is thrown when xml document does not
		 * conform to the xml schema after attribute value change
		 */
		[Test]
		[ExpectedException (typeof (XmlSchemaValidationException))]
		public void XAttributeThrowExceptionValidate ()
		{
			String elementName = "note";
			String attributeName =  "date";
			object attributeValue =  "2010-12-32";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler ( TestValidationHandler),true);
			Assert.AreEqual (true, validationSucceded);

			// change and re-validate attribute value
			XAttribute date = xmlDocument.Element (elementName).Attribute (attributeName);
			date.SetValue (attributeValue);
			ExtensionsClass.Validate (date, date.GetSchemaInfo ().SchemaAttribute, schemaSet, null);
		}

		// element validation succeeds after change
		[Test]
		[Category ("NotWorking")]
		public void XElementSuccessValidate ()
		{
			String parentElementName = "note";
			String childElementName = "body";
			object childElementValue = "Please call me!";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), true);
			Assert.AreEqual (true, validationSucceded);

			// alter element
			XElement root = xmlDocument.Element (parentElementName);
			root.SetElementValue (childElementName, childElementValue);

			ExtensionsClass.Validate (root, root.GetSchemaInfo ().SchemaElement, schemaSet, 
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (true, validationSucceded, "XElementSuccessValidate");
		
		}

		// element validation fails after change
		[Test]
		[Category ("NotWorking")]
		public void XElementFailValidate ()
		{
			String parentElementName = "note";
			String childElementName = "body";
			object childElementValue = "Don't forget to call me! Please!";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), true);
			Assert.AreEqual (true, validationSucceded);

			// alter element
			XElement root = xmlDocument.Element (parentElementName);
			root.SetElementValue (childElementName, childElementValue);

			ExtensionsClass.Validate (root, root.GetSchemaInfo ().SchemaElement, schemaSet, 
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (false, validationSucceded, "XElementFailValidate");

		}

		/* 
		 * test if exception is thrown when xml document does not
		 * conform to the xml schema after element value change
		 */
		[Test]
		[ExpectedException (typeof (XmlSchemaValidationException))]
		[Category ("NotWorking")]
		public void XElementThrowExceptionValidate ()
		{
			String parentElementName = "note" ;
			String childElementName = "body";
			object childElementValue = "Don't forget to call me! Please!";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), true);
			Assert.AreEqual (true, validationSucceded);

			// alter element
			XElement root = xmlDocument.Element (parentElementName);
			root.SetElementValue (childElementName, childElementValue);

			ExtensionsClass.Validate (root, root.GetSchemaInfo ().SchemaElement, schemaSet, null);
		}

		// test attribute schema info
		[Test]
		[Category ("NotWorking")]
		public void XAttributeGetSchemaInfo ()
		{
			String elementName =  "note";
			String attributeName = "date";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), true);
			Assert.AreEqual (true, validationSucceded);

			// validate attribute
			XAttribute date = xmlDocument.Element (elementName).Attribute (attributeName);
			ExtensionsClass.Validate (date, date.GetSchemaInfo ().SchemaAttribute, schemaSet, 
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (true, validationSucceded);

			IXmlSchemaInfo schemaInfo =  ExtensionsClass.GetSchemaInfo (date);
			Assert.IsNotNull (schemaInfo, "XAttributeGetSchemaInfo");
		}

		// test element schema info
		[Test]
		[Category ("NotWorking")]
		public void XElementGetSchemaInfo ()
		{
			String elementName = "body";

			// validate the entire document
			validationSucceded = true;
			ExtensionsClass.Validate (xmlDocument, schemaSet, 
				new ValidationEventHandler (TestValidationHandler), true);
			Assert.AreEqual (true, validationSucceded);

			// validate element
			XElement body = xmlDocument.Root.Element (elementName);
			ExtensionsClass.Validate (body, body.GetSchemaInfo ().SchemaElement, schemaSet,  
				new ValidationEventHandler (TestValidationHandler));
			Assert.AreEqual (true, validationSucceded);

			IXmlSchemaInfo schemaInfo = ExtensionsClass.GetSchemaInfo (body);
			Assert.IsNotNull (schemaInfo, "ElementGetSchemaInfo");
		}

	}
}

#endif