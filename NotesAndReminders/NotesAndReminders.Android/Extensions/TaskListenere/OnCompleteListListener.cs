using Firebase.Firestore;
using NotesAndReminders.Droid.Extensions;
using NotesAndReminders.Droid.Services;
using NotesAndReminders.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace NotesAndReminders.Droid.Extensions.TaskListenere
{
	public class OnCompleteListListener<T> : Java.Lang.Object, Android.Gms.Tasks.IOnCompleteListener where T : Identifiable, IDBItem
	{
		private TaskCompletionSource<List<IDBItem>> _onCompleteCallback;

		public OnCompleteListListener(TaskCompletionSource<List<IDBItem>> onCompleteCallback)
		{
			_onCompleteCallback = onCompleteCallback;
		}
		private static T Convert(DocumentSnapshot doc)
		{
			Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings
			{
				TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
				Formatting = Newtonsoft.Json.Formatting.Indented
			};
			try
			{
				var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(doc.Data.ToDictionary(), settings);
				var item = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonStr, settings);
				item.Id = doc.Id;
				if (item is NoteType nt)
				{
					var data = (JObject)JsonConvert.DeserializeObject(jsonStr, settings);
					var lcolor = data["noteColorLight"].Value<string>();
					var dcolor = data["noteColorDark"].Value<string>();
					nt.Color = new NoteColorModel();

					var color = Color.FromHex(lcolor);
					nt.Color.Light = color;

					color = Color.FromHex(dcolor);
					nt.Color.Dark = color;

					return (T)System.Convert.ChangeType(nt, typeof(T));
				}
				else
				{
					return item;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(ex.StackTrace);

				throw;
			}
		}

		public async void OnComplete(Android.Gms.Tasks.Task task)
		{
			if (task.IsSuccessful)
			{
				var models = new List<IDBItem>();

				var collObj = task.Result;
				if (collObj is QuerySnapshot collQuery)
				{

					foreach (var item in collQuery.Documents)
					{
						var convertedItem = Convert(item);
						if (convertedItem is Note note)
						{
							if (note.NoteTypeId != null)
							{
								FirebaseCloudFirestoreService service = new FirebaseCloudFirestoreService();
								var nt = new NoteType();
								await service.GetNoteTypeAsync(note.NoteTypeId, notetype =>
								{
									nt = notetype as NoteType;
								});

								note.Type = nt;
							}

							models.Add(note);
						}
						else
						{
							models.Add(convertedItem);
						}

					}

					_onCompleteCallback.TrySetResult(models);
				}
				else
				{
					throw new Exception("No such collection exists");
				}
			}
			else
			{
				throw new Exception("Failed to get collection");
			}
		}
	}
}