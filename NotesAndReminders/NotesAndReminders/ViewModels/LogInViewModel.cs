using NotesAndReminders.Exceptions;
using NotesAndReminders.Services;
using NotesAndReminders.Views;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace NotesAndReminders.ViewModels
{
	public class LogInViewModel : BaseViewModel
	{
		private IAuthorizationService _authorizationService;

		private string _email;
		private string _password;
		private bool _isLoggingIn;

		public string Email
		{
			get => _email;
			set => SetProperty(ref _email, value);
		}
		public string Password
		{
			get => _password;
			set => SetProperty(ref _password, value);
		}
		public bool IsLoggingIn
		{
			get => _isLoggingIn;
			set => SetProperty(ref _isLoggingIn, value);
		}

		public ICommand LogInCommand { get; private set; }
		public ICommand SignUpCommand { get; private set; }

		public LogInViewModel()
		{
			_authorizationService = DependencyService.Get<IAuthorizationService>();

			LogInCommand = new Command(LogInAsync);
			SignUpCommand = new Command(SignUpAsync);
		}

		private async void LogInAsync()
		{
			if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
			{
				MessagingCenter.Send(this, Constants.EmptyLoginOrPasswordEvent);
				return;
			}

			try
			{
				IsLoggingIn = true;
				var res = await _authorizationService.LogIn(Email, Password);

				Settings.User = res.Item1;
				Settings.UserToken = res.Item2;

				MessagingCenter.Send(this, Constants.LoggedInEvent);
			}
			catch (InvalidCredentialsException ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);

				MessagingCenter.Send(this, Constants.InvalidLoginOrPasswordEvent);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);

				MessagingCenter.Send(this, Constants.UnexpectedErrorEvent);
			}
			finally
			{
				IsLoggingIn = false;
			}
		}

		private async void SignUpAsync()
		{
			await Shell.Current.GoToAsync(nameof(SignUpView));
		}
	}
}
