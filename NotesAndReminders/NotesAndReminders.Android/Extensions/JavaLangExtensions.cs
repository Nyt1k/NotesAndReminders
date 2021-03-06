using Android.App;
using Android.Content;
using Android.Icu.Text;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase;
using Java.Util;
using NotesAndReminders.Droid.Services;
using NotesAndReminders.Models;
using NotesAndReminders.Services;
using NotesAndReminders.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Forms;

namespace NotesAndReminders.Droid.Extensions
{
	public static class JavaLangExtensions
	{
		public static IDictionary<string, object> ToDictionary(this IDictionary<string, Java.Lang.Object> map)
		{
			var dict = new Dictionary<string, object>();

			foreach (var key in map.Keys)
			{

				var val = map[key];

				if (key.Equals("noteColorLight") || key.Equals("noteColorDark"))
				{
					var color = val.ToString();
					dict.Add(key, color);
				}
				else if (key.Equals("state"))
				{
					dict.Add(key, Enum.Parse(typeof(NoteState), val.ToString()));
				}
				else if (key.Equals("last_time_modifired"))
				{
					DateTime dt = DateTime.ParseExact((string)val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
					dict.Add(key, dt);
				}
				else if (key.Equals("notification_time"))
				{
					DateTime dt = DateTime.ParseExact((string)val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
					dict.Add(key, dt);
				}
				else if (key.Equals("checklist"))
				{
					var list = new List<ChecklistItem>();
					var dicChecklist = new JavaDictionary<string, bool>(val.Handle, JniHandleOwnership.DoNotRegister);

					foreach (var pair in dicChecklist)
					{
						var text = pair.Key;
						var check = pair.Value;
						list.Add(new ChecklistItem() { Text = text, IsChecked = check });
					}

					dict.Add(key, list);
				}
				else if (key.Equals("addition_content"))
				{
					var list = new Dictionary<string, string>();

					var dicImage = new JavaDictionary<string, string>(val.Handle, JniHandleOwnership.DoNotRegister);

					foreach (var pair in dicImage)
					{
						var name = pair.Key;
						var url = pair.Value;
						list.Add(name, url);
					}

					var listUrls = new Dictionary<string, byte[]>();

					WebClient webClient = new WebClient();

					foreach (var image in list)
					{
						listUrls.Add(image.Key, webClient.DownloadData(image.Value));
					}

					dict.Add(key, listUrls);
				}
				else if (val is Java.Lang.String str)
				{
					dict.Add(key, str.ToString());
				}
				else if (val is Java.Lang.Double dbl)
				{
					dict.Add(key, dbl.DoubleValue());
				}
				else if (val is Java.Lang.Integer intVal)
				{
					dict.Add(key, intVal.IntValue());
				}
				else if (val is Java.Lang.Boolean boolVal)
				{
					dict.Add(key, boolVal.BooleanValue());
				}
				else if(val is Java.Lang.Long longNumber)
				{
					dict.Add(key, (long)longNumber);
				}
				else if (val is System.Collections.ICollection coll)
				{
					var arrList = new ArrayList(coll);
					var list = new List<string>();
					for (int i = 0; i < arrList.Size(); i++)
					{
						list.Add(arrList.Get(i).ToString());
					}
					dict.Add(key, list);
				}
				else
				{
					dict.Add(key, val.ToString());
				}
			}

			return dict;
		}

		public static Dictionary<string, Java.Lang.Object> Convert(this Dictionary<string,object> item)
		{
			var dict = new Dictionary<string, Java.Lang.Object>();

			var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(item);
			var propDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);

			foreach(var key in propDict.Keys)
			{

				var val = propDict[key];
				Java.Lang.Object javaVal = null;

				if(val is string str)
				{
					javaVal = new Java.Lang.String(str);
				}
				else if(val is double dbl)
				{
					javaVal = new Java.Lang.Double(dbl);
				}
				else if(val is int intVal)
				{
					javaVal = new Java.Lang.Integer(intVal);
				}
				else if(val is DateTime dt)
				{
					javaVal = dt.ToString("yyyy-MM-dd HH:mm:ss");
				}
				else if(val is bool boolVal)
				{
					javaVal = new Java.Lang.Boolean(boolVal);
				}
				else if(key.Equals("addition_content") && val is Newtonsoft.Json.Linq.JObject jarray)
				{
					var list = jarray.ToObject<Dictionary<string,string>>();

					HashMap map = new HashMap();

					foreach(var listitem in list)
					{
						map.Put(new Java.Lang.String(listitem.Key), new Java.Lang.String(listitem.Value));
					}

					javaVal = map;
				}
				else if(val is Newtonsoft.Json.Linq.JArray jArray)
				{
					List<ChecklistItem> list = jArray.ToObject<List<ChecklistItem>>();
					HashMap map = new HashMap();

					foreach(var listitem in list)
					{
						map.Put(new Java.Lang.String(listitem.Text), new Java.Lang.Boolean(listitem.IsChecked));
					}

					javaVal = map;
				}else if(val is long longnumber)
				{
					javaVal = new Java.Lang.Long(longnumber);
				}
				
				if(javaVal != null)
				{
					dict.Add(key, javaVal);
				}
			}

			return dict;
		}


		
	}
}