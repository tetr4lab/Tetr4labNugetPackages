## tetr4lab. private libraries for Blazor

```csharp
using Tetr4lab;
using Tetr4lab.Auth;
using Tetr4lab.Db;
using Tetr4lab.Db.Mysql;
using Tetr4lab.Db.Sqlite;
using Tetr4lab.Razor;
using Tetr4lab.Server;
using Tetr4lab.Wasm;
```

### NuGet Packages for Blazor
```
https://github.com/tetr4lab/Tetr4labNugetPackages.git?path=/Packages
```
#### Assemblies and Clases
##### Tetr4lab
TaskEx,
RevisionInfo,
ParameterHelper,
HttpClientHelper,
CollectionHelper,
StringHelper,

##### Tetr4labAuth
Account,
AuthStateProvider,
AuthStateHelper,
AuthedIdentity,
IServiceCollectionHelper,

##### Tetr4labDatabase
IBaseModel,
BaseModel<T>,
BasicDataSet,
ExceptionToErrorHelper,
DatabaseHelper,
enum Status,
Result<T>,
StatusHelper,
VirtualColumnAttribute,

##### Tetr4labMySqlDatabase
MyDataSetException,
MySqlDatabase,
MySqlDataSet,

##### Tetr4labSqliteDatabase
SqliteDataSetExeption,
SqliteDatabase,
SqliteDataSet,

##### Tetr4labRazor
ConfirmationDialog,
MudDialogServiceHelper,
JSRuntimeHelper,
NavigationManagerHelper,
ProgressDialog,
ProgressDialogHelper,
SessionCounter,

##### Tetr4labServer
ProtectedLocalStrageHelper,

##### Tetr4labWasm
CookieHandler,
IServiceCollectionHelper,

### C# Additions for Unity Package Manager
```
https://github.com/tetr4lab/Tetr4labNugetPackages.git?path=/Tetr4lab
```
