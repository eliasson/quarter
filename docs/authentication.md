By default Quarter is running in local mode. This is ideal for:

- Development
- Running solo on you localhost

When running local mode a fix user will be used named _User_ and a static ID (`47ba567a-711e-4c4a-a7b0-07756d965a79`).

To run in local mode, set `Application.LocalMode` to `true`.

### Identity providers

For all other purposes you should configure an external identity provider. Currently only GitHub and
Google Open ID Connect is supported, but feel free to open an issue and PR to add additional provider.

The identity provider is only used to authenticate the users. That will not gain them access
to Quarter. In order to do that they must be added as local users with a matching e-mail
address (this is done int the admin UI).

To run using an identity provider, set `Application.LocalMode` to `false` and add your ID's
and secrets in the application configuration (make sure not to commit these!):

```
  "Auth": {
    "Providers": {
      "GitHub": {
        "ClientId": "",
        "ClientSecret": ""
      },
      "Google": {
        "ClientId": "",
        "ClientSecret": ""
      }
    }
  },
```

You can setup your own OpenID Connect client:

* [Google Cloud Platform](https://console.cloud.google.com/apis/credentials?pli=1)
* [GitHub](https://docs.github.com/en/developers/apps/building-oauth-apps/creating-an-oauth-app)

#### Initial user

During Quarters boot phase an initial user will be created if configured. Add your e-mail
and don't forget to set `Enabled` to `true` under the `InitialUser` section.

````json
  "InitialUser": {
    "Enabled": true,
    "Email": "jane.doe@example.com"
  }
````

Once the initial user is created additional users can be added in the Admin section of the
application UI.
