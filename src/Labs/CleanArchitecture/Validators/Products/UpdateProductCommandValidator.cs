using FluentValidation;
using VK.Lab.CleanArchitecture.Commands.Products;

namespace VK.Lab.CleanArchitecture.Validators.Products
{
    /// <summary>
    /// UpdateProductCommand 验证器
    /// </summary>
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Product ID must be greater than zero");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(1, 100).WithMessage("Product name must be between 1 and 100 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero");
        }
    }
}
