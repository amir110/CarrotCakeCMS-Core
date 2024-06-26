﻿using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

/*
* CarrotCake CMS (MVC Core)
* http://www.carrotware.com/
*
* Copyright 2015, 2023, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: June 2023
*/

namespace Carrotware.Web.UI.Components {

	public static class ReflectionUtilities {

		public static BindingFlags PublicInstanceStatic {
			get {
				return BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
			}
		}

		public static Type GetTypeFromString(string classString) {
			Type? typ = null;
			if (!string.IsNullOrEmpty(classString)) {
				typ = Type.GetType(classString);

				//if (typ == null && classString.IndexOf(",") < 1) {
				//	typ = BuildManager.GetType(classString, true);
				//}
			}

			return typ;
		}

		public static object GetPropertyValue(object obj, string property) {
			PropertyInfo propertyInfo = obj.GetType().GetProperty(property, PublicInstanceStatic);
			return propertyInfo.GetValue(obj, null);
		}

		public static object GetPropertyValueFlat(object obj, string property) {
			PropertyInfo[] propertyInfos = obj.GetType().GetProperties(PublicInstanceStatic);
			PropertyInfo propertyInfo = null;
			foreach (PropertyInfo info in propertyInfos) {
				if (info.Name == property) {
					propertyInfo = info;
					break;
				}
			}
			if (propertyInfo != null) {
				return propertyInfo.GetValue(obj, null);
			} else {
				return null;
			}
		}

		public static bool DoesPropertyExist(object obj, string property) {
			PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
			return propertyInfo == null ? false : true;
		}

		public static bool DoesPropertyExist(Type type, string property) {
			PropertyInfo propertyInfo = type.GetProperty(property);
			return propertyInfo == null ? false : true;
		}

		public static List<string> GetPropertyStrings(object obj) {
			List<string> props = (from i in GetProperties(obj)
								  orderby i.Name
								  select i.Name).ToList();
			return props;
		}

		public static List<PropertyInfo> GetProperties(object obj) {
			PropertyInfo[] info = obj.GetType().GetProperties(PublicInstanceStatic);

			List<PropertyInfo> props = (from i in info.AsEnumerable()
										orderby i.Name
										select i).ToList();
			return props;
		}

		public static List<string> GetPropertyStrings(Type type) {
			List<string> props = (from i in GetProperties(type)
								  orderby i.Name
								  select i.Name).ToList();
			return props;
		}

		public static List<PropertyInfo> GetProperties(Type type) {
			PropertyInfo[] info = type.GetProperties(PublicInstanceStatic);

			List<PropertyInfo> props = (from i in info.AsEnumerable()
										orderby i.Name
										select i).ToList();
			return props;
		}

		public static string GetPropertyString(Type type, string PropertyName) {
			string prop = (from i in GetProperties(type)
						   where i.Name.ToLowerInvariant().Trim() == PropertyName.ToLowerInvariant().Trim()
						   orderby i.Name
						   select i.Name).FirstOrDefault();
			return prop;
		}

		public static PropertyInfo GetProperty(Type type, string PropertyName) {
			PropertyInfo prop = (from i in GetProperties(type)
								 where i.Name.ToLowerInvariant().Trim() == PropertyName.ToLowerInvariant().Trim()
								 orderby i.Name
								 select i).FirstOrDefault();
			return prop;
		}

		public static string GetDescriptionAttribute(Type type, string fieldName) {
			PropertyInfo property = GetProperty(type, fieldName);
			if (property != null) {
				foreach (Attribute attr in property.GetCustomAttributes(typeof(DescriptionAttribute), true)) {
					if (attr != null) {
						DescriptionAttribute description = (DescriptionAttribute)attr;
						return description.Description;
					}
				}
			}

			return string.Empty;
		}

		public static PropertyInfo PropInfoFromExpression<T>(this T source, Expression<Func<T, object>> expression) {
			string propertyName = string.Empty;
			PropertyInfo propInfo = null;

			MemberExpression memberExpression = expression.Body as MemberExpression ??
												((UnaryExpression)expression.Body).Operand as MemberExpression;
			if (memberExpression != null) {
				propertyName = memberExpression.Member.Name;

				switch (memberExpression.Expression.NodeType) {
					case ExpressionType.MemberAccess:
						string propName = ExtBuildProp(memberExpression);
						propInfo = typeof(T).GetProperty(propName);
						break;

					default:
						propInfo = typeof(T).GetProperty(propertyName);
						break;
				}
			}

			return propInfo;
		}

		public static object GetPropValueFromExpression<T>(this T item, Expression<Func<T, object>> property) {
			string columnName = ReflectionUtilities.BuildProp(property);
			PropertyInfo propInfo = item.PropInfoFromExpression<T>(property);
			object val = propInfo.GetValue(item, null);
			object obj = null;

			if (columnName.Contains(".")) {
				columnName = columnName.Substring(columnName.IndexOf(".") + 1);

				foreach (string colName in columnName.Split('.')) {
					obj = GetPropertyValue(val, colName);
					val = obj;
				}
			} else {
				obj = val;
			}

			return obj;
		}

		public static object GetPropValueFromColumnName<T>(this T item, string columnName) {
			PropertyInfo propInfo = null;
			object obj = null;
			object val = null;

			if (columnName.Contains(".")) {
				foreach (string colName in columnName.Split('.')) {
					if (val == null) {
						obj = GetPropertyValue(item, colName);
					} else {
						obj = GetPropertyValue(val, colName);
					}
					val = obj;
				}
			} else {
				propInfo = GetProperty(typeof(T), columnName);
				val = propInfo.GetValue(item, null);
				obj = val;
			}

			return obj;
		}

		public static string BuildProp<T>(Expression<Func<T, object>> property) {
			MemberExpression memberExpression = property.Body as MemberExpression ??
											((UnaryExpression)property.Body).Operand as MemberExpression;

			Expression expression = property.Body;
			string propertyName = string.Empty;

			if (memberExpression.NodeType == ExpressionType.MemberAccess) {
				expression = memberExpression;
			}

			while (expression.NodeType == ExpressionType.MemberAccess) {
				memberExpression = ((MemberExpression)expression);
				expression = memberExpression.Expression;

				if (string.IsNullOrEmpty(propertyName)) {
					propertyName = memberExpression.Member.Name;
				} else {
					propertyName = string.Format("{0}.{1}", memberExpression.Member.Name, propertyName);
				}
			}

			if (expression.NodeType != ExpressionType.MemberAccess) {
				if (string.IsNullOrEmpty(propertyName)) {
					propertyName = memberExpression.Member.Name;
				}
			}

			return propertyName;
		}

		public static string ExtBuildProp(Expression expression) {
			MemberExpression memberExpression = expression as MemberExpression ??
												((UnaryExpression)expression).Operand as MemberExpression;

			if (memberExpression.NodeType == ExpressionType.MemberAccess) {
				expression = memberExpression;
			}

			string propertyName = string.Empty;

			while (expression.NodeType == ExpressionType.MemberAccess) {
				memberExpression = ((MemberExpression)expression);
				expression = memberExpression.Expression;
			}

			if (expression.NodeType != ExpressionType.MemberAccess) {
				if (string.IsNullOrEmpty(propertyName)) {
					propertyName = memberExpression.Member.Name;
				}
			}

			return propertyName;
		}

		private static Expression ParseExpression(Expression expression) {
			while (expression.NodeType == ExpressionType.MemberAccess) {
				expression = ((MemberExpression)expression).Expression;
			}

			if (expression.NodeType != ExpressionType.MemberAccess) {
				return (ParameterExpression)expression;
			}

			return null;
		}

		public static object GetAttribute<T>(Type type, string memberName) {
			MemberInfo[] memInfo = type.GetMember(memberName);

			if (memInfo != null && memInfo.Length > 0) {
				foreach (var m in memInfo) {
					object[] attrs = m.GetCustomAttributes(typeof(T), false);

					if (attrs != null && attrs.Length > 0) {
						return ((T)attrs[0]);
					}
				}
			}

			return null;
		}

		public static IQueryable<T> SortByParm<T>(this IList<T> source, string sortByFieldName, string sortDirection) {
			return SortByParm<T>(source.AsQueryable(), sortByFieldName, sortDirection);
		}

		public static IEnumerable<T> SelectPage<T, T2>(this IQueryable<T> list, Func<T, T2> sortFunc,
				string sortDirection, int page, int pageSize) {
			var isDescending = (sortDirection ?? "").ToUpperInvariant() == "DESC";

			return SelectPage(list, sortFunc, sortDirection, page, pageSize);
		}

		public static IEnumerable<T> SelectPage<T, T2>(this IQueryable<T> list, Func<T, T2> sortFunc,
			bool isDescending, int page, int pageSize) {
			List<T>? result;
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : pageSize;
			var skip = (page - 1) * pageSize;

			if (isDescending) {
				result = list.OrderByDescending(sortFunc).Skip(skip).Take(pageSize).ToList();
			} else {
				result = list.OrderBy(sortFunc).Skip(skip).Take(pageSize).ToList();
			}

			return result;
		}

		public static IEnumerable<T> PaginateList<T>(this IQueryable<T> list, int page, int pageSize) {
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : pageSize;
			var skip = (page - 1) * pageSize;

			return list.Skip(skip).Take(pageSize).ToList();
		}

		public static IEnumerable<T> PaginateListFromZero<T>(this IQueryable<T> list, int page, int pageSize) {
			page = page < 0 ? 0 : page;
			pageSize = pageSize < 1 ? 10 : pageSize;
			var skip = page * pageSize;

			return list.Skip(skip).Take(pageSize).ToList();
		}

		public static IQueryable<T> SortByParm<T>(this IQueryable<T> source, string sortByFieldName, string sortDirection) {
			sortDirection = string.IsNullOrEmpty(sortDirection) ? "ASC" : sortDirection.Trim().ToUpperInvariant();

			string SortDir = sortDirection.Contains("DESC") ? "OrderByDescending" : "OrderBy";

			Type type = typeof(T);
			ParameterExpression parameter = Expression.Parameter(type, "source");

			PropertyInfo? property;
			Expression? propertyAccess;

			if (sortByFieldName.Contains('.')) {
				//handles complex child properties
				string[] childProps = sortByFieldName.Split('.');
				property = type.GetProperty(childProps[0]);
				propertyAccess = Expression.MakeMemberAccess(parameter, property);
				for (int i = 1; i < childProps.Length; i++) {
					property = property.PropertyType.GetProperty(childProps[i]);
					propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
				}
			} else {
				property = type.GetProperty(sortByFieldName);
				propertyAccess = Expression.MakeMemberAccess(parameter, property);
			}

			LambdaExpression orderByExp = Expression.Lambda(propertyAccess, parameter);

			MethodCallExpression resultExp = Expression.Call(typeof(Queryable), SortDir, new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));

			return source.Provider.CreateQuery<T>(resultExp);
		}
	}
}