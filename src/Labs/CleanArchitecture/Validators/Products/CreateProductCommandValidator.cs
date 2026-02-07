using FluentValidation;
using VK.Lab.CleanArchitecture.Commands.Products;

namespace VK.Lab.CleanArchitecture.Validators.Products
{
    /// <summary>
    /// CreateProductCommand 验证器
    /// </summary>
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(1, 100).WithMessage("Product name must be between 1 and 100 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero");
        }
    }
}
