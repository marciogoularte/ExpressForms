﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExpressForms.Buttons;
using ExpressForms.Inputs;
using System.Reflection;

namespace ExpressForms
{
    public class ExpressFormsController<T, TId> : Controller
        where T : class, new()
    {        
        protected IExpressFormsExchange<T, TId> Exchange { get; set; }

        public ExpressFormsController() 
        {
            // Set default values to properties
            IndexViewName = "ExpressFormsIndex";
            EditorViewName = "ExpressFormsEditor";
            CustomPropertyNames = new Dictionary<string, string>();
            CustomPropertyDisplay = new Dictionary<string, Func<dynamic, string>>();
            CustomEditorInputs = new Dictionary<string, ExpressFormsInput>();
            IgnoredPropertyNames = new string[] { };            
        }

        protected void Initialize(IExpressFormsExchange<T, TId> exchange)
        {
            Exchange = exchange;                        
        }

        #region properties and functions that are used to customize the appearance and behavior of forms
        protected string FormName { get { return typeof(T).Name; } }
        
        protected virtual ExpressFormsButton[] IndexButtons
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// A virtual helper function that is meant to be used to get the buttons to display on the editor form.
        /// May be overridden in a derived class to change what buttons appear.
        /// </summary>
        /// <param name="model"></param>        
        /// <param name="isNew"></param>
        protected virtual ExpressFormsButton[] GetEditorButtons(bool isNew)
        {
            List<ExpressFormsButton> buttons = new List<ExpressFormsButton>()
            {
                new ExpressFormsModifyDataButton()
                {
                    // If the user is inserting a new record, show the user an [Insert] button.    
                    IsVisible = isNew,
                    Text = "Save",
                    FormName = FormName,
                    ActionType = ExpressFormsModifyDataButton.ActionTypeEnum.Insert,
                    PostUrl = Url.Action("Postback"),
                    PostType = ExpressFormsModifyDataButton.PostTypeEnum.Ajax
                },
                new ExpressFormsModifyDataButton()
                {
                    // If the user is updating an existing record, show the user an [Update] button.
                    IsVisible = !isNew,
                    Text = "Save",
                    FormName = FormName,
                    ActionType = ExpressFormsModifyDataButton.ActionTypeEnum.Update,
                    PostUrl = Url.Action("Postback"),
                    PostType = ExpressFormsModifyDataButton.PostTypeEnum.Ajax
                }                
            };
            return buttons.ToArray();
        } // end GetEditorButtons

        // A virtual helper function that is meant to be used to get the inputs that appear on the form.
        // May be overridden in a derived class to customize how the form works.
        protected virtual Dictionary<string, ExpressFormsInput> GetEditorInputs(T record)
        {
            // t.GetType() is used here rather than typeof(T) in order to get the most specific type implemented by the object passed in.
            IEnumerable<PropertyInfo> properties = record.GetType().GetProperties()
                .Where(p => (IgnoredPropertyNames==null) || !IgnoredPropertyNames.Contains(p.Name));

            Func<PropertyInfo, ExpressFormsInput> InputSelector = p => GetEditorInput(record, p);

            Dictionary<string, ExpressFormsInput> inputs = properties
                .ToDictionary(p => p.Name, InputSelector);

            return inputs;
        }

        private ExpressFormsInput GetEditorInput(T record, PropertyInfo property)
        {
            ExpressFormsInput input;

            string inputName = property.Name;
            string value = Convert.ToString(property.GetValue(record, null));
            bool isVisible = true;

            input = GetCustomEditorInput(inputName, value, isVisible);

            if (input == null) // we didn't get an input from GetCustomEditorInput
            {
                switch (property.PropertyType.Name)
                {
                    case "Boolean":
                        input = new ExpressFormsCheckBox()
                        {
                            FormName = FormName,
                            InputName = inputName,
                            Value = value,
                            IsVisible = isVisible,
                        };
                        break;
                    default:
                        input = new ExpressFormsTextBox()
                        {
                            FormName = FormName,
                            InputName = inputName,
                            Value = value,
                            IsVisible = isVisible
                        };
                        break;
                }
            }

            // If this property has an associated "CustomPropertyName", use that for the display name.  Otherwise, use the inputName.
            input.InputDisplayName = CustomPropertyNames.Keys.Contains(input.InputName) ? CustomPropertyNames[input.InputName] : input.InputName;

            return input;
        }

        protected virtual ExpressFormsInput GetCustomEditorInput(string inputName, string value, bool isVisible)
        {
            ExpressFormsInput customInput;            

            // If there is a custom input with matching inputName, assign it the value of the input passed in and return it.
            if (CustomEditorInputs.Keys.Contains(inputName))
            {
                customInput = CustomEditorInputs[inputName];
                customInput.Value = value;
                return customInput;
            }
            // Otherwise, return null.
            else            
                return null;
        }

        protected Dictionary<string, ExpressFormsInput> CustomEditorInputs { get; set; }

        protected string IndexTitle { get; set; }        
        protected Dictionary<string, string> CustomPropertyNames { get; set; }
        protected IEnumerable<string> IgnoredPropertyNames { get; set; }
        protected Dictionary<string, Func<dynamic, string>> CustomPropertyDisplay { get; set; }

        protected string IndexViewName { get; set; }
        protected string EditorViewName { get; set; }

        #endregion

        #region public methods that render views
        /// <summary>
        /// Returns a ViewResult to display an "Index" view from which the user may select a row to edit (or view details that may be hidden)
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Index()
        {
            ExpressFormsIndexViewModel model = new ExpressFormsIndexViewModel()
            {
                RecordType = typeof(T),
                Title = IndexTitle == null ? this.GetType().Name : IndexTitle,
                CustomIndexHeaders = CustomPropertyNames,
                CustomPropertyDisplay = CustomPropertyDisplay,

                Records = Exchange.Get().ToArray()
            };

            return View(IndexViewName, model);
        }

        /// <summary>
        /// Returns a ViewResult to display an "Editor" form from which the user can insert or update data.
        /// </summary>
        /// <param name="id">the ID of the row to update; if null, the user may insert a new row.</param>        
        public virtual ActionResult Editor(TId id)
        {            
            T record = (id == null) ? new T() : Exchange.Get(id);
            bool isNew = id == null;

            ExpressFormsEditorModel model = new ExpressFormsEditorModel()
            {
                Record = record,
                IsNew = isNew,
                Buttons = GetEditorButtons(isNew),
                Inputs = GetEditorInputs(record)
            };

            return View(EditorViewName, model);
        }
        #endregion                

        #region methods that modify the data when called

        /// <summary>
        /// Postback is a single point of entry for the client to update server-side data.
        /// This method may be called via either form post or AJAX as the page designates.        
        /// Declared virtual so that other developers may specify alternative implementation.
        /// </summary>
        /// <param name="record">the record that the user wants to insert/update/delete</param>
        /// <param name="actionType">'INSERT', 'UPDATE', or 'DELETE'</param>
        /// <param name="postType">'AJAX' or 'FORM' to tell the server how to respond</param>
        /// <returns></returns>
        [ValidateInput(false)]
        public virtual ActionResult Postback(T record, string actionType, string postType)
        {
            // Check that a valid postType was specified
            if (postType == null || !new []{"AJAX", "FORM"}.Contains(postType.ToUpper()))
                throw new ArgumentOutOfRangeException("Must specify 'AJAX' or 'FORM' for postType, encountered: " + postType);

            // Check that a valid action was specified.
            if (actionType == null || !new[] { "INSERT", "UPDATE", "DELETE" }.Contains(actionType.ToUpper()))
                throw new ArgumentOutOfRangeException("Must specify 'INSERT', 'UPDATE', or 'DELETE' for actionType, encountered: " + actionType);

            OperationResult result = null;
            switch (actionType.ToUpper())
            {
                case "INSERT":
                    result = Insert(record);
                    break;
                case "UPDATE":
                    result = Update(record);
                    break;
                case "DELETE":
                    // TODO: This depends on the ID field being called "Id".  This needs to be fixed so that the ID is properly looked up.
                    TId id = (TId)(((dynamic)(record)).Id);
                    result = Delete(id);
                    break;
            }

            switch(postType.ToUpper())
            {
                case "AJAX":
                    return Json(result);                    
                case "FORM":
                    return Redirect(Request.UrlReferrer.ToString());                                
                default:
                    throw new InvalidOperationException();
            }
        } // End Postback

        private OperationResult Insert(T record)
        {
            TId id = Exchange.Insert(record);
            return new OperationResult()
            {
                Result = "Insert OK",
                Id = id
            };
        }

        private OperationResult Update(T record)
        {
            Exchange.Update(record);
            return new OperationResult()
            {
                Result = "Update OK",
            };
        }

        private OperationResult Delete(TId Id)
        {
            Exchange.Delete(Id);
            return new OperationResult()
            {
                Result = "Delete OK",
            };
        }

        private class OperationResult
        {
            public string Result { get; set; }
            public TId Id { get; set; }
        }

        #endregion methods that modify the data when called
    }
}