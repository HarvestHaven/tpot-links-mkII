using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CodeMechanic.TrashStack;

/*
 * CODE FROM: https://www.learnrazorpages.com/advanced/render-partial-to-string
 */
public interface IRenderingService
{
    Task<string> RenderAsync<TModel>(string partialName, TModel model);
}

public class RenderingService : IRenderingService
{
    private IRazorViewEngine _viewEngine;
    private ITempDataProvider _tempDataProvider;
    private IServiceProvider _serviceProvider;

    public RenderingService(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderAsync<TModel>(string partialName, TModel model)
    {
        var actionContext = GetActionContext();
        var partial = FindView(actionContext, partialName);
        using (var output = new StringWriter())
        {
            var viewContext = new ViewContext(
                actionContext,
                partial,
                new ViewDataDictionary<TModel>(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(
                    actionContext.HttpContext,
                    _tempDataProvider),
                output,
                new HtmlHelperOptions()
            );
            await partial.RenderAsync(viewContext);
            return output.ToString();
        }
    }

    private IView FindView(ActionContext actionContext, string partialName)
    {
        var getPartialResult = _viewEngine.GetView(null, partialName, false);
        if (getPartialResult.Success)
        {
            return getPartialResult.View;
        }

        var findPartialResult = _viewEngine.FindView(actionContext, partialName, false);
        if (findPartialResult.Success)
        {
            return findPartialResult.View;
        }

        var searchedLocations = getPartialResult.SearchedLocations.Concat(findPartialResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find partial '{partialName}'. The following locations were searched:" }.Concat(
                searchedLocations));
        ;
        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }
}

public interface IEmailService
{
    Task SendAsync(string email, string name, string subject, string body);
}

public class DemoEmailService : IEmailService
{
    public async Task SendAsync(string email, string name, string subject, string body)
    {
        using (var smtp = new SmtpClient())
        {
            smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            string pickupdir = @"/home/nick/Downloads/maildump/";
            if (!Directory.Exists(pickupdir))
                Directory.CreateDirectory(pickupdir);
            
            smtp.PickupDirectoryLocation = pickupdir;
            var message = new MailMessage
            {
                Body = body,
                Subject = subject,
                From = new MailAddress(email, name),
                IsBodyHtml = true
            };
            message.To.Add("contact@domain.com");
            await smtp.SendMailAsync(message);
        }
    }
}