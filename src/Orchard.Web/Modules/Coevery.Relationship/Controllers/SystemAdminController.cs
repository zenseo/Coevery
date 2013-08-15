﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Coevery.Core.Services;
using Coevery.Relationship.Records;
using Coevery.Relationship.Services;
using Coevery.Relationship.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using Orchard.Core.Contents.Controllers;

namespace Coevery.Relationship.Controllers {
    public class SystemAdminController : Controller, IUpdateModel {
        private readonly IRelationshipService _relationshipService;

        public SystemAdminController(IOrchardServices orchardServices,
            IRelationshipService relationshipService) {
            Services = orchardServices;
            _relationshipService = relationshipService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [HttpPost]
        public ActionResult FieldNames(string entityName, int version) {
            if (string.IsNullOrWhiteSpace(entityName) || entityName == "0") {
                return Json(new {
                    result = "<option value=''>  </option>",
                    version = version
                });
            }

            var optionsHtml = new StringBuilder();
            foreach (var option in _relationshipService.GetFieldNames(entityName)) {
                optionsHtml.Append("<option value='"+option.Value);
                if (option.Selected) {
                    optionsHtml.Append("' selected = 'selected");
                }
                optionsHtml.Append("'>" + option.Text + "</option>");
            }
            return Json(new {
                result = optionsHtml.ToString(),
                version = version
            });
        }

        public ActionResult Relationships() {
            return View();
        }

        #region OneToMany
        public ActionResult CreateOneToMany(string id) {
            if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Not allowed to edit a content.")))
                return new HttpUnauthorizedResult();

            return View(new OneToManyRelationshipModel {
                EntityList = _relationshipService.GetEntityNames(id),
                PrimaryEntity = id,
                IsCreate = true
            });
        }

        public ActionResult EditOneToMany(string entityName, int relationId) {
            if (!Services.Authorizer.Authorize(Permissions.EditContent, T("Not allowed to edit a content.")))
                return new HttpUnauthorizedResult();
            
            var oneToMany = _relationshipService.GetOneToMany(relationId);
            if (oneToMany == null || oneToMany.Id == 0) {
                return ResponseError("Relationship not found");
            }
            return View("CreateOneToMany",new OneToManyRelationshipModel {
                IsCreate = false,
                Name = oneToMany.Relationship.Name,
                DeleteOption = (OneToManyDeleteOption)oneToMany.DeleteOption,
                PrimaryEntity = oneToMany.Relationship.PrimaryEntity.Name,
                RelatedEntity = oneToMany.Relationship.RelatedEntity.Name,
                RelatedListLabel = oneToMany.RelatedListLabel,
                ShowRelatedList = oneToMany.ShowRelatedList,
            });
        }

        [HttpPost]
        public ActionResult CreateOneToMany(OneToManyRelationshipModel oneToMany) {
            if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Not allowed to edit a content.")))
                return new HttpUnauthorizedResult();

            if (oneToMany.IsCreate) {
                
                var errorMessage = _relationshipService.CreateRelationship(oneToMany);
                if (!string.IsNullOrWhiteSpace(errorMessage)) {
                    ModelState.AddModelError("OneToManyRelation", T(errorMessage).ToString());
                    return ResponseError("");
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult EditOneToMany(int relationId, OneToManyRelationshipModel oneToMany) {
            if (!Services.Authorizer.Authorize(Permissions.EditContent, T("Not allowed to edit a content."))) {
                return new HttpUnauthorizedResult();
            }
            var errorMessage = _relationshipService.EditRelationship(relationId, oneToMany);
            if (!string.IsNullOrWhiteSpace(errorMessage)) {
                ModelState.AddModelError("ManyToManyRelation", T(errorMessage).ToString());
                return ResponseError("");
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        #endregion

        #region ManyToMany
        public ActionResult CreateManyToMany(string id) {
            if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Not allowed to edit a content.")))
                return new HttpUnauthorizedResult();

            return View(new ManyToManyRelationshipModel {
                EntityList = _relationshipService.GetEntityNames(id),
                PrimaryEntity = id,
                IsCreate = true
            });
        }

        public ActionResult EditManyToMany(string entityName, int relationId) {
            if (!Services.Authorizer.Authorize(Permissions.EditContent, T("Not allowed to edit a content.")))
                return new HttpUnauthorizedResult();

            var manyToMany = _relationshipService.GetManyToMany(relationId);
            if (manyToMany == null || manyToMany.Id == 0) {
                return ResponseError("Relationship not found");
            }

            return View("CreateManyToMany",new ManyToManyRelationshipModel {
                IsCreate = false,
                Name = manyToMany.Relationship.Name,
                PrimaryEntity = manyToMany.Relationship.PrimaryEntity.Name,
                RelatedEntity = manyToMany.Relationship.RelatedEntity.Name,
                PrimaryListLabel = manyToMany.PrimaryListLabel,
                RelatedListLabel = manyToMany.RelatedListLabel,
                ShowPrimaryList = manyToMany.ShowPrimaryList,
                ShowRelatedList = manyToMany.ShowRelatedList,
            });
        }

        [HttpPost]
        public ActionResult CreateManyToMany(ManyToManyRelationshipModel manyToMany) {
            if (!Services.Authorizer.Authorize(Permissions.PublishContent, T("Not allowed to edit a content.")))
                return new HttpUnauthorizedResult();

            if (manyToMany.IsCreate) {
                var errorMessage = _relationshipService.CreateRelationship(manyToMany);
                if (!string.IsNullOrWhiteSpace(errorMessage)) {
                    ModelState.AddModelError("ManyToManyRelation", T(errorMessage).ToString());
                    return ResponseError("");
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult EditManyToMany(int relationId, ManyToManyRelationshipModel manyToMany) {
            if (!Services.Authorizer.Authorize(Permissions.EditContent, T("Not allowed to edit a content."))) {
                return new HttpUnauthorizedResult();
            }
            var errorMessage = _relationshipService.EditRelationship(relationId,manyToMany);
            if (!string.IsNullOrWhiteSpace(errorMessage)) {
                ModelState.AddModelError("ManyToManyRelation", T(errorMessage).ToString());
                return ResponseError("");
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        #endregion

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return base.TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        private ActionResult ResponseError(string errorMessage) {
            Services.TransactionManager.Cancel();
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var temp = (from values in ModelState
                        from error in values.Value.Errors
                        select error.ErrorMessage).ToArray();
            return Content(string.Join("\n",temp)+";\n"+errorMessage);
        }
    }
}