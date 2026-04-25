using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "اسم المستخدم مطلوب")]
		[Display(Name = "اسم المستخدم")]
		public string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
		[EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
		[Display(Name = "البريد الإلكتروني")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "كلمة المرور مطلوبة")]
		[MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
		[DataType(DataType.Password)]
		[Display(Name = "كلمة المرور")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "كلمة المرور غير متطابقة")]
		[Display(Name = "تأكيد كلمة المرور")]
		public string ConfirmPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "الاسم الكامل مطلوب")]
		[Display(Name = "الاسم الكامل")]
		public string FullName { get; set; } = string.Empty;

		[Required(ErrorMessage = "رقم الهاتف مطلوب")]
		[Display(Name = "رقم الهاتف")]
		[Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
		public string PhoneNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "العملة مطلوبة")]
		[Display(Name = "العملة المفضلة")]
		public string CurrencyCode { get; set; } = "EGP"; // EGP, SAR

		[Required(ErrorMessage = "اسم المتجر/الشركة مطلوب")]
		[Display(Name = "اسم المتجر/الشركة")]
		[StringLength(100, ErrorMessage = "اسم المتجر لا يجب أن يتجاوز 100 حرف")]
		public string StoreName { get; set; } = string.Empty;

		[Display(Name = "عنوان المؤسسة")]
		[StringLength(200, ErrorMessage = "العنوان لا يجب أن يتجاوز 200 حرف")]
		public string? StoreAddress { get; set; }
	}
}
