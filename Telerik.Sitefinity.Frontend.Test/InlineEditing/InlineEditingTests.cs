﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Telerik.Sitefinity.Frontend.InlineEditing;
using Telerik.Sitefinity.Utilities.HtmlParsing;
using Telerik.Sitefinity.Frontend.InlineEditing.Attributes;

namespace Telerik.Sitefinity.Frontend.Test.InlineEditing
{
    /// <summary>
    /// Tests the CommonExtensions class.
    /// </summary>
    [TestClass]
    public class InlineEditingTests
    {
        [TestMethod]
        [Owner("Bonchev")]
        [Description("Checks whether the HtmlRegion class properly closes a given region when disposed")]
        public void HtmlRegion_CreateFakeHtmlContentInRegion_IsRegionproperlyClosed()
        {
            //Arrange: create text writer and dummy content 
            TextWriter writer = new StringWriter();

            string dummyContent = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit";

            //the HTML tag which must be used as a region wrapper must be of type - div
            string htmlWrapperTag = "div";

            string dummyHtmlContent = string.Format("<{0}>{1}", htmlWrapperTag, dummyContent);

            //Act: create the HTML region 
            using (new HtmlRegion(writer, htmlWrapperTag))
            {
                writer.WriteLine(dummyHtmlContent);
            }
            var outPut = writer.ToString();

            //Assert: Parses the generated by the HtmlRegion HTML checks if the HTML content is properly wrapped into a region and if this region is properly closed
            using (HtmlParser parser = new HtmlParser(outPut))
            {
                var chunk = parser.ParseNext();
                Assert.IsNotNull(chunk);

                //checks if the HTML tag is of type div
                Assert.IsTrue(chunk.TagName.Equals(htmlWrapperTag, StringComparison.InvariantCultureIgnoreCase));

                string content = null;
                HtmlChunk nextChunk = null;
                while ((nextChunk = parser.ParseNext()) != null)
                {
                    chunk = nextChunk;
                    if (nextChunk.Type == HtmlChunkType.Text)
                    {
                        content = nextChunk.GenerateHtml();
                    }
                }

                //checks if the region inner content is what it should be
                Assert.IsTrue(content.Contains(dummyContent));

                //checks if the region is properly closed
                Assert.IsTrue(chunk.TagName.Equals(htmlWrapperTag, StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(chunk.Type == HtmlChunkType.CloseTag);
            }
        }

        [TestMethod]
        [Owner("Bonchev")]
        [Description("Checks whether CreateInlineEditingRegion method of the HtmlProcessor class properly wraps a given HTML content into a InlineEditing region.")]
        public void HtmlProcessor_CreateInlineEditingRegion_IsDummyContentwrappedIntoInlineEditingRegion()
        {
            //Arrange: create dummy data which will be set to the related attributes inside the region div tag
            TextWriter writer = new StringWriter();
            string providerName = "dummyProvider";
            string type = "dummyType";
            var id = Guid.NewGuid();
            string dummyContent = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit";

            var providerAttribute = "data-sf-provider";
            var typeAttribute = "data-sf-type";
            var idAttribute = "data-sf-id";

            //Act: create the HTML region 
            var htmlProcessor = new HtmlProcessor();
            using (htmlProcessor.CreateInlineEditingRegion(writer, providerName, type, id))
            {
                writer.WriteLine(dummyContent);
            }
            var outPut = writer.ToString();

            //Assert: Parses the generated by the htmlTransformationProxy HTML checks if the HTML content is properly wrapped into a div tag
            //which has the required by the InlineEditing attributes
            //and these attributes has a proper data assigned
            using (HtmlParser parser = new HtmlParser(outPut))
            {
                var chunk = parser.ParseNext();
                Assert.IsNotNull(chunk);

                //checks if the HTML tag is of type div and if it has the required attributes
                Assert.IsTrue(chunk.TagName.Equals("div", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(chunk.HasAttribute(idAttribute));
                Assert.IsTrue(chunk.HasAttribute(providerAttribute));
                Assert.IsTrue(chunk.HasAttribute(typeAttribute));

                //checks if the required attributes has proper values assigned to them
                Assert.AreEqual<string>(providerName, chunk.GetParamValue(providerAttribute));
                Assert.AreEqual<string>(type, chunk.GetParamValue(typeAttribute));
                Assert.AreEqual<string>(id.ToString(), chunk.GetParamValue(idAttribute));

                string content = null;
                HtmlChunk nextChunk = null;
                while ((nextChunk = parser.ParseNext()) != null)
                {
                    chunk = nextChunk;
                    if (nextChunk.Type == HtmlChunkType.Text)
                    {
                        content = nextChunk.GenerateHtml();
                    }
                }

                //checks if the region inner content is what it should be
                Assert.IsTrue(content.StartsWith(dummyContent, StringComparison.InvariantCultureIgnoreCase));

                //checks if the region is properly closed
                Assert.IsTrue(chunk.TagName.Equals("div", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(chunk.Type == HtmlChunkType.CloseTag);
            }
        }

        [TestMethod]
        [Owner("Bonchev")]
        [Description("Checks whether GetText method of the HtmlProcessor class properly wraps a given HTML content into a InlineEditing region for a property of the model which is marked with FieldInfoAttribute.")]
        public void HtmlProcessor_GetText_TextElelementProperlyCreatedForInlineEditng()
        {

            string dummyContent = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit";
            var htmlProcessor = new HtmlProcessor();
            var dummyWidgetModel = new DummyWidgetModel { EditableContent = dummyContent, NonEditableContent = dummyContent };

            string fieldName = "DummyWidget";
            string type = "LongText";
            var fieldAttribute = "data-sf-field";
            var typeAttribute = "data-sf-ftype";

            var inlineeditingAwareContent = htmlProcessor.GetStringContent(dummyWidgetModel, "EditableContent");
            var nonInlineeditingAwareContent = htmlProcessor.GetStringContent(dummyWidgetModel, "NonEditableContent");

            Assert.AreEqual<string>(dummyContent, nonInlineeditingAwareContent);

            using (HtmlParser parser = new HtmlParser(inlineeditingAwareContent))
            {
                var chunk = parser.ParseNext();
                Assert.IsNotNull(chunk);

                //checks if the HTML tag is of type div and if it has the required attributes
                Assert.IsTrue(chunk.TagName.Equals("div", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(chunk.HasAttribute(fieldAttribute));
                Assert.IsTrue(chunk.HasAttribute(typeAttribute));

                //checks if the required attributes has proper values assigned to them
                Assert.AreEqual<string>(fieldName, chunk.GetParamValue(fieldAttribute));
                Assert.AreEqual<string>(type, chunk.GetParamValue(typeAttribute));

                string content = null;
                HtmlChunk nextChunk = null;
                while ((nextChunk = parser.ParseNext()) != null)
                {
                    chunk = nextChunk;
                    if (nextChunk.Type == HtmlChunkType.Text)
                    {
                        content = nextChunk.GenerateHtml();
                    }
                }

                //checks if the region inner content is what it should be
                Assert.IsTrue(content.StartsWith(dummyContent, StringComparison.InvariantCultureIgnoreCase));

                //checks if the region is properly closed
                Assert.IsTrue(chunk.TagName.Equals("div", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(chunk.Type == HtmlChunkType.CloseTag);
            }
        }
    }

    /// <summary>
    /// This class is used for a fake widget property model one of its properties is marked with FieldInfoAttribute and the other is not
    /// </summary>
    public class DummyWidgetModel
    {
        /// <summary>
        /// Gets or sets the HTML which must be wrapped into InlineEditing region.
        /// </summary>
        [FieldInfo("DummyWidget", "LongText")]
        public string EditableContent { get; set; }

        /// <summary>
        /// Gets or sets the non editable content.
        /// </summary>
        public string NonEditableContent { get; set; }
    }

}