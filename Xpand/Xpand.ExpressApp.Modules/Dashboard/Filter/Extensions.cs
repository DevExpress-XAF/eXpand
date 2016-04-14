﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DevExpress.DashboardCommon;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using Xpand.ExpressApp.Dashboard.BusinessObjects;
using Xpand.Persistent.Base.Xpo;
using Xpand.Utils.Helpers;

namespace Xpand.ExpressApp.Dashboard.Filter {
    public static class Extensions {
        public static void SaveDashboard(this DevExpress.DashboardCommon.Dashboard dashboard, IDashboardDefinition template, MemoryStream memoryStream,XafApplication application) {
            dashboard.SynchronizeModel(template,application);
            dashboard.SaveToXml(memoryStream);
            dashboard.ApplyModel(FilterEnabled.Always, template,application.Model);
        }

        public static DevExpress.DashboardCommon.Dashboard CreateDashBoard(this IDashboardDefinition template, FilterEnabled filterEnabled, Func<Type, object> dashboardDatasource, XafApplication application) {
            var dashBoard = CreateDashBoard(template,dashboardDatasource, application);
            if (dashBoard != null){
                dashBoard.ApplyModel(filterEnabled, template,application.Model);
            }
            return dashBoard;
        }

        public static DevExpress.DashboardCommon.Dashboard CreateDashBoard(this IDashboardDefinition template, Func<Type, object> dashboardDatasource, XafApplication xafApplication) {
            var dashboard = new DevExpress.DashboardCommon.Dashboard();
            try {
                if (!string.IsNullOrEmpty(template.Xml)) {
                    dashboard = LoadFromXml(template);
                }
                foreach (var typeWrapper in template.DashboardTypes.Select(wrapper 
                    => new { wrapper.Type, Caption = wrapper.GetDefaultCaption((ModelApplicationBase)xafApplication.Model) })) {
                    var wrapper = typeWrapper;
                    var dsource = dashboard.DataSources.FirstOrDefault(source => source.Name.Equals(wrapper.Caption));
                    object objects = dashboardDatasource(typeWrapper.Type);
                    if (dsource != null) {
                        dsource.Data = objects;
                    }
                    else if (!dashboard.DataSources.Contains(ds => ds.Name.Equals(wrapper.Caption))) {
                        dashboard.DataSources.Add(new DashboardObjectDataSource(typeWrapper.Caption,objects));
                    }
                }
            }
            catch (Exception e) {
                dashboard.Dispose();
                Tracing.Tracer.LogError(e);
            }
            return dashboard;
        }

        static DevExpress.DashboardCommon.Dashboard LoadFromXml(IDashboardDefinition dashboardDefinition) {
            var dashboard = new DevExpress.DashboardCommon.Dashboard();
            using (var me = new MemoryStream()) {
                var sw = new StreamWriter(me);
                sw.Write(dashboardDefinition.Xml);
                sw.Flush();
                me.Seek(0, SeekOrigin.Begin);
                dashboard.LoadFromXml(me);
                sw.Close();
                me.Close();
            }
            return dashboard;
        }

        public static void SynchronizeModel(this DevExpress.DashboardCommon.Dashboard dashboard,  IDashboardDefinition template,XafApplication application) {
            var dataSources = GetDataSources(dashboard, FilterEnabled.Always, template,application.Model);
            foreach (var dataSource in dataSources){
                var filter = dataSource.ModelDataSource as IModelDashboardDataSourceFilter;
                if (filter != null)
                    dataSource.DataSource.Filter = filter.SynchronizeFilter(dataSource.DataSource.Filter);
            }
        }

        public static string GetXml(this IDashboardDefinition template, FilterEnabled filterEnabled, Func<Type, object> dashboardDatasource, XafApplication xafApplication) {
            var dashBoard = template.CreateDashBoard( filterEnabled,dashboardDatasource, xafApplication);
            using (var memoryStream = new MemoryStream()) {
                dashBoard.SaveToXml(memoryStream);
                memoryStream.Position = 0;
                var document = XDocument.Load(memoryStream);
                var dataSourceAdapters = GetDataSources(dashBoard, filterEnabled, template,xafApplication.Model);
                foreach (var dataSourceAdapter in dataSourceAdapters){
                    if (document.Root != null){
                        DataSourceAdapter adapter = dataSourceAdapter;
                        var datasources = document.Root.Descendants("DataSource").Where(element => element.Attribute("Name").Value==adapter.DataSource.Name&&!element.Descendants("Filter").Any());
                        foreach (var datasource in datasources){
                            datasource.Add(new XElement("Filter", dataSourceAdapter.DataSource.Filter));    
                        }
                    }
                }
                return document.ToString();
            }
        }

        public static void ApplyModel(this DevExpress.DashboardCommon.Dashboard dashboard, FilterEnabled filterEnabled, IDashboardDefinition template, IModelApplication model) {
            var dataSources = GetDataSources(dashboard, filterEnabled, template,model);
            foreach (var adapter in dataSources) {
                var filter = adapter.ModelDataSource as  IModelDashboardDataSourceFilter;
                if (filter != null) adapter.DataSource.Filter = filter.ApplyFilter(adapter.DataSource.Filter);
                var parameter = adapter.ModelDataSource as IModelDashboardDataSourceParameter;
                if (parameter != null) parameter.ApplyValue(dashboard.Parameters[parameter.ParameterName]);
            }
        }

        static IEnumerable<DataSourceAdapter> GetDataSources(DevExpress.DashboardCommon.Dashboard dashboard, FilterEnabled filterEnabled, IDashboardDefinition template, IModelApplication model) {
            var modelDashboardModule =
                ((IModelApplicationDashboardModule)model).DashboardModule;
            return modelDashboardModule.DataSources.Where(source => source.NodeEnabled && CanApply(source, filterEnabled, template)).Select(modelDataSource => {
                var dataSource = dashboard.DataSources.FirstOrDefault(source =>
                    String.Equals(source.Name.Trim(), modelDataSource.DataSourceName.Trim(), StringComparison.CurrentCultureIgnoreCase));
                return new DataSourceAdapter(dataSource, modelDataSource);
            }).Where(adapter => adapter.ModelDataSource != null&&adapter.DataSource!=null);
        }

        private static bool CanApply(IModelDashboardDataSource modelDashboardDataSource, FilterEnabled filterEnabled, IDashboardDefinition template) {
            if (modelDashboardDataSource.NodeEnabled&&new[]{FilterEnabled.Always,filterEnabled}.Contains(modelDashboardDataSource.Enabled)) {
                var objectSpace = XPObjectSpace.FindObjectSpaceByObject(template);
                var isObjectFitForCriteria = objectSpace.IsObjectFitForCriteria(template, CriteriaOperator.Parse(modelDashboardDataSource.DashboardDefinitionCriteria));
                return isObjectFitForCriteria.HasValue && isObjectFitForCriteria.Value;
            }
            return false;
        }


        public static string SynchronizeFilter(this IModelDashboardDataSourceFilter modelDataSource, string filter){
            return !string.IsNullOrEmpty(filter) ? Regex.Replace(filter, "( and )??" + modelDataSource.Filter + "( and )?", "", RegexOptions.IgnoreCase) : null;
        }

        public static void ApplyValue(this IModelDashboardDataSourceParameter parameter, DashboardParameter dashboardParameter){
            if (parameter.IsCustomFunction) {
                if (dashboardParameter!=null) {
                    var criteriaOperator = CriteriaOperator.Parse("Field="+ parameter.ParameterValue);
                    var customFunctionValueProcessor = new CustomFunctionValueProcessor();
                    customFunctionValueProcessor.Process(criteriaOperator);
                    dashboardParameter.Value = ((OperandValue) ((BinaryOperator) criteriaOperator).RightOperand).Value;
                }
            }
            else {
                object result;
                var tryToChange = parameter.ParameterValue.TryToChange(dashboardParameter.Type, out result);
                if (tryToChange)
                    dashboardParameter.Value = result;
            }
        }

        public static string ApplyFilter(this IModelDashboardDataSourceFilter modelDataSource, string filterString) {
            string criteria = null;
            if (!string.IsNullOrEmpty(modelDataSource.Filter)) {
                var criteriaOperator = CriteriaOperator.Parse(modelDataSource.Filter);
                var customFunctionValueProcessor = new CustomFunctionValueProcessor();
                customFunctionValueProcessor.Process(criteriaOperator);
                criteria = criteriaOperator.ToString();
                if (!string.IsNullOrEmpty(filterString))
                    criteria = " and " + criteria;
            }
            return filterString + criteria;
        }
    }

    class DataSourceAdapter {
        private readonly IDashboardDataSource _dataSource;

        private readonly IModelDashboardDataSource _dashboardDataSource;

        public DataSourceAdapter(IDashboardDataSource dataSource, IModelDashboardDataSource dashboardDataSource) {
            _dataSource = dataSource;
            _dashboardDataSource = dashboardDataSource;
        }

        public IDashboardDataSource DataSource {
            get { return _dataSource; }
        }

        public IModelDashboardDataSource ModelDataSource {
            get { return _dashboardDataSource; }
        }
    }

}
