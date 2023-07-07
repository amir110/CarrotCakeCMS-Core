﻿using Carrotware.CMS.Interface;
using Carrotware.CMS.Interface.Controllers;
using Carrotware.Web.UI.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Northwind.Data;
using System.Data;

namespace Northwind.Controllers {

	[WidgetController(typeof(AdminController))]
	public class AdminController : BaseAdminWidgetController {
		protected NorthwindContext db = new NorthwindContext();

		protected readonly IWebHostEnvironment _webHostEnvironment;
		protected readonly ICarrotSite _site;

		public AdminController(IWebHostEnvironment environment, ICarrotSite site) {
			if (db == null) {
				db = new NorthwindContext();
			}

			_site = site;
			_webHostEnvironment = environment;
		}

		public new void Dispose() {
			base.Dispose();

			if (db != null) {
				db.Dispose();
			}
		}

		// GET: Admin
		public ActionResult Products() {
			PagedData<Product> model = new PagedData<Product>();
			model.InitOrderBy(x => x.ProductName);
			model.PageSize = 20;

			var srt = model.ParseSort();
			var query = db.Products.SortByParm(srt.SortField, srt.SortDirection);

			model.DataSource = query.PaginateList(model.PageNumber, model.PageSize).ToList();
			model.TotalRecords = db.Products.Count();
			ViewBag.SupplierList = db.Suppliers.ToList();

			ViewBag.SiteID = this.SiteID;

			return View(model);
		}

		[HttpPost]
		public ActionResult Products(PagedData<Product> model) {
			model.ToggleSort();
			var srt = model.ParseSort();
			var query = db.Products.SortByParm(srt.SortField, srt.SortDirection);

			model.DataSource = query.PaginateList(model.PageNumber, model.PageSize).ToList();
			model.TotalRecords = db.Products.Count();
			ViewBag.SupplierList = db.Suppliers.ToList();

			ModelState.Clear();

			ViewBag.SiteID = this.SiteID;

			return View(model);
		}

		public ActionResult Suppliers() {
			ViewBag.SiteID = _site.SiteID;

			PagedData<Supplier> model = new PagedData<Supplier>();
			model.InitOrderBy(x => x.CompanyName);
			model.PageSize = 10;

			var srt = model.ParseSort();
			var query = db.Suppliers.SortByParm(srt.SortField, srt.SortDirection);

			model.DataSource = query.PaginateList(model.PageNumber, model.PageSize).ToList();
			model.TotalRecords = db.Suppliers.Count();

			return View(model);
		}

		[HttpPost]
		public ActionResult Suppliers(PagedData<Supplier> model) {
			ViewBag.SiteID = _site.SiteID;

			model.ToggleSort();
			var srt = model.ParseSort();
			var query = db.Suppliers.SortByParm(srt.SortField, srt.SortDirection);

			model.DataSource = query.PaginateList(model.PageNumber, model.PageSize).ToList();
			model.TotalRecords = db.Suppliers.Count();

			ModelState.Clear();

			return View(model);
		}

		public ActionResult ViewSupplier(int id) {
			return View(db.Suppliers.Where(x => x.SupplierId == id).FirstOrDefault());
		}

		protected void LoadDDLs() {
			ViewBag.SupplierList = db.Suppliers.ToList();
			ViewBag.CategoryList = db.Categories.ToList();
		}

		public ActionResult EditProduct(int id) {
			LoadDDLs();

			return View(db.Products.Where(x => x.ProductId == id).FirstOrDefault());
		}

		public ActionResult CreateProduct() {
			LoadDDLs();
			return View("EditProduct", new Product());
		}

		[HttpPost]
		public ActionResult EditProduct(Product model) {
			return CreateProduct(model);
		}

		[HttpPost]
		public ActionResult CreateProduct(Product model) {
			LoadDDLs();

			Product prod = db.Products.Where(x => x.ProductId == model.ProductId).FirstOrDefault();

			if (prod == null) {
				prod = model;
				db.Products.Add(prod);
			} else {
				prod.ProductName = model.ProductName;
				prod.UnitPrice = model.UnitPrice;
				prod.UnitsInStock = model.UnitsInStock;
				prod.UnitsOnOrder = model.UnitsOnOrder;
				prod.ReorderLevel = model.ReorderLevel;
				prod.Discontinued = model.Discontinued;
			}

			db.SaveChanges();

			return View("EditProduct", new Product());
		}

		public ActionResult Employees() {
			PagedData<Employee> model = new PagedData<Employee>();
			model = InitEmpData();

			return View(model);
		}

		[HttpPost]
		public ActionResult Employees(PagedData<Employee> model) {
			model = GetEmpData(model);

			ModelState.Clear();

			return View(model);
		}

		private PagedData<Employee> InitEmpData() {
			PagedData<Employee> model = new PagedData<Employee>();
			model.InitOrderBy(x => x.LastName, true);
			model.PageSize = 5;

			model.DataSource = (from c in db.Employees.Include(x => x.EmployeeTerritories)
								orderby c.LastName ascending
								select c).Take(model.PageSize).ToList();

			model.TotalRecords = (from c in db.Employees
								  select c).Count();

			ViewBag.TerritoryList = db.Territories.ToList();

			return model;
		}

		private PagedData<Employee> GetEmpData(PagedData<Employee> model) {
			List<Employee> lst = new List<Employee>();
			model.PageSize = 5;

			model.ToggleSort();
			var srt = model.ParseSort();

			model.DataSource = new List<Employee>();
			IQueryable<Employee> query = (from c in db.Employees.Include(x => x.EmployeeTerritories) select c);

			query = query.SortByParm(srt.SortField, srt.SortDirection);

			model.DataSource = query.PaginateList(model.PageNumber, model.PageSize).ToList();

			model.TotalRecords = (from c in db.Employees select c).Count();

			ViewBag.TerritoryList = db.Territories.ToList();

			model.SortByNew = string.Empty;

			return model;
		}

		public ActionResult ViewEmployee(int id) {
			var model = (from c in db.Employees
						 where c.EmployeeId == id
						 select c).FirstOrDefault();

			return View(model);
		}

		public ActionResult Products2() {
			PagedDataTable model = new PagedDataTable();
			model.PageSize = 10;
			model.InitOrderBy("ProductID");

			return ProductDataSet(model);
		}

		[HttpPost]
		public ActionResult Products2(PagedDataTable model) {
			return ProductDataSet(model);
		}

		public ActionResult Products3() {
			PagedDataTable model = new PagedDataTable();
			model.PageSize = 10;
			model.InitOrderBy("ProductName");
			ViewBag.SupplierList = db.Suppliers.ToList();

			return ProductDataSet(model);
		}

		[HttpPost]
		public ActionResult Products3(PagedDataTable model) {
			ViewBag.SupplierList = db.Suppliers.ToList();

			return ProductDataSet(model);
		}

		[HttpPost]
		public ActionResult ProductDataSet(PagedDataTable model) {
			model.ToggleSort();
			var srt = model.ParseSort();

			string queryText = "SELECT Count(*) RowCt FROM [dbo].[Products] " +
								"  \r\n" +
								"SELECT ProductID, ProductName, SupplierID, CategoryID, QuantityPerUnit, UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discontinued \r\n" +
								"FROM (SELECT *, " +
								"	(ROW_NUMBER() " +
								"	OVER ( ORDER BY " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'ProductID' THEN ProductID END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'ProductName' THEN ProductName END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'SupplierID' THEN SupplierID END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'CategoryID' THEN CategoryID END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'QuantityPerUnit' THEN QuantityPerUnit END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'UnitPrice' THEN UnitPrice END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'UnitsInStock' THEN UnitsInStock END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'UnitsOnOrder' THEN UnitsOnOrder END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'ReorderLevel' THEN ReorderLevel END ASC, " +
								"			CASE when @srtDir = 'ASC' AND @srt = 'Discontinued' THEN Discontinued END ASC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'ProductID' THEN ProductID END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'ProductID' THEN ProductID END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'ProductName' THEN ProductName END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'SupplierID' THEN SupplierID END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'CategoryID' THEN CategoryID END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'QuantityPerUnit' THEN QuantityPerUnit END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'UnitPrice' THEN UnitPrice END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'UnitsInStock' THEN UnitsInStock END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'UnitsOnOrder' THEN UnitsOnOrder END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'ReorderLevel' THEN ReorderLevel END DESC, " +
								"			CASE when @srtDir = 'DESC' AND @srt = 'Discontinued' THEN Discontinued END DESC " +
								"		)) as RowID  \r\n" +
								"FROM [dbo].[Products] ) as tbl \r\n" +
								"WHERE RowID BETWEEN @startRow AND @endRow ";

			Dictionary<string, string> dict = new Dictionary<string, string>();
			dict.Add("srt", srt.SortField);
			dict.Add("srtDir", srt.SortDirection);
			dict.Add("startRow", (1 + (model.PageNumberZeroIndex * model.PageSize)).ToString());
			dict.Add("endRow", (model.PageNumber * model.PageSize).ToString());

			List<SqlParameter> parms = BuildParms(dict);

			DataSet ds = ExecDataSet(queryText, parms);

			model.TotalRecords = Convert.ToInt32(ds.Tables[0].Rows[0]["RowCt"]);
			model.SetData(ds.Tables[1]);

			ModelState.Clear();

			return View(model);
		}

		protected DataSet ExecDataset(string queryText) {
			return ExecDataSet(queryText, null);
		}

		protected DataSet ExecDataSet(string queryText, List<SqlParameter> parms) {
			string sConnectionString = CarrotHttpHelper.Configuration.GetConnectionString("NorthwindConnection").ToString();
			DataSet ds = new DataSet();

			if (parms == null) {
				parms = new List<SqlParameter>();
			}

			using (SqlConnection cn = new SqlConnection(sConnectionString)) {
				using (SqlCommand cmd = new SqlCommand(queryText, cn)) {
					cn.Open();
					cmd.CommandType = CommandType.Text;

					if (parms != null) {
						foreach (var p in parms) {
							cmd.Parameters.Add(p);
						}
					}

					using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
						da.Fill(ds);
					}
				}
				cn.Close();
			}

			return ds;
		}

		protected List<SqlParameter> BuildParms(Dictionary<string, string> dict) {
			List<SqlParameter> parms = new List<SqlParameter>();

			foreach (var d in dict) {
				SqlParameter p = new SqlParameter();
				p.ParameterName = string.Format("@{0}", d.Key);
				p.SqlDbType = SqlDbType.VarChar;
				p.Size = 2048;
				p.Direction = ParameterDirection.Input;
				p.Value = d.Value;

				parms.Add(p);
			}

			return parms;
		}

		//=======================
		protected override void Dispose(bool disposing) {
			if (db != null) {
				db.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}