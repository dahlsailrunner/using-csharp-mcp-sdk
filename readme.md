## Getting Started

Make sure the `CarvedRock-Aspire.AppHost` project is set as the startup
project.

Run it!

> **N O T E:** The first time you run the app, it may take a little longer to start
if you don't already have the Postgres container images downloaded.

The Aspire Dashboard will be launched and that will have links for the different
projects.  Start by clicking the link for the WebApp project!

## Features

This is a simple e-commerce application for learning purposes - specifically around MCP Servers.

Here are the features:

- **MCP**
  - 4 tools are in the MCP server
  - The MCP Inspector is included via an Aspire hosting package to make interactively testing the MCP server easy
  - `CarvedRockTools.cs` contains 2 tools that can be called anonymously (if `RequireAuthorization`()
     is not included on the `MapMcp()` call in `Program.cs`
  - `AdminTools.cs` contains 2 tools that will require a user with the `admin` role both to see when listing tools
     and to execute them
  - OAuth is implemented for security, with a local instance of the 
    [demo Duende IdentityServer](https://github.com/DuendeSoftware/demo.duendesoftware.com) due to a 
    couple of changes needed for it that the [public hosted demo](https://demo.duendesoftware.com) 
    does not include.
  - Automated tests are included in the `tests` folder that will test the MCP server.

- **API**
  - `GET` based on category (or "all") and by id allow anonymous requests
  - `POST`, `PUT`, and `DELETE` require authentication and an `admin` role (available with the `bob` login, but not `alice`)
  - Validation will be done with [FluentValidation](https://docs.fluentvalidation.net/en/latest/index.html) and errors returned as a `400 Bad Request` with `ProblemDetails`
  - A `GET` with a category of something other than "all", "boots", "equip", or "kayak" will return a `500 internal server error` with `ProblemDetails`
  - Data is seeded by the `SeedData.json` contents in the `Data` project
- Authentication provided by OIDC via a [demo instance of Duende IdentityServer](https://demo.duendesoftware.com) (`bob` is an admin, `alice` is not)

- **WebApp**
  - The home page and listing pages will show a subset of products
  - There is a page at `/Admin` that will show a list of products that can be edited / deleted and new ones added
  - If you navigate to `/Admin` without the admin role, you should see an `AccessDenied` page
  - Any validation errors from the API should be displayed on the admin section add page
  - Can add items to cart and see a summary of the cart (shows when empty too)
  - Can submit an order or cancel the order and clear the cart
  - A submitted order will send a fake email

## VS Code Setup

Running in VS Code is a totally legitimate use-case for this solution and
repo.

The same instructions above (Getting Started) apply here, but the following
extension should probably be installed (it includes some other extensions):

- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

Then just hit `F5` to run the app.

## Data and EF Core Migrations

The `dotnet ef` tool is used to manage EF Core migrations.  The following command was used to create migrations (from the `CarvedRock.Data` folder).

```bash
dotnet ef migrations add Initial -s ../CarvedRock.Api
```

The application uses PostgreSQL.

## MailKit Client

Added `MailKit.Client` project based on the tutorial here:
<https://learn.microsoft.com/en-us/dotnet/aspire/extensibility/custom-integration>

## Verifying Emails

The very simple email functionality is done using a template
from [this GitHub repo](https://github.com/leemunroe/responsive-html-email-template)
and the [MailPit](https://mailpit.axllent.org/)
service that can easily run in a Docker container.

To see the emails just hit the link for the `smtp` service in the Aspire Dashboard.
