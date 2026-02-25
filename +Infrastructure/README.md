# Типа пример как можно юзать SimpleInjector при разработке аддонов под *CAD

Первое что делаю - несколько проектов:
- Abstractions : интерфейсы, абстрактные классы, перечисления...
- Logic : типа логика (в т.ч. и ViewModel'и). Связана с Abstractions. Пока тоже отсутствует, нам же надо с SimpleInject разобраться ;)
- NCad : загружается в наник. Имеет свои реализации того, что нужно из Abstractions, вызывает Logic и Views (последнего тут не будет, поскольку проект тепличный)
- Views (отсутствует) : окна, UC. Связано с Logic и Abstractions

## Abstractions
Проект NET Standard 2.0, чтоб можно было и в что-то древнее воткнуть. Объявляю два интерфейса - информации о текущей сборке и сервиса сообщений, и на этом успокаиваюсь.

На текущий момент какие-то абстракции уже есть, можно делать и реализации под *CAD. Чисто на поржать.

## NCAD 

Создаю отдельный проект под NET6, добавляю NuGet-пакеты nanoCAD (на виртуалке установлен только 23.0 и 24.0, так что беру 23.0) и SimpleInjector последней версии. Наврал, дома на виртуалке 23.1, 24.1, 25.1. Так что обновляю пакет до 23.1 и настраиваю запуск соответствующим образом ;)

Теперь реализации сервиса сообщений и информации о сборке. Делаю только консольный вывод, поскольку выдачу MessageBox надо будет ковырять отдельно.

Теперь можно и команды прописывать. Консольное сообщение - просто вывод чего-то, информационное - вывод типа в MessageBox. Второе, понятное дело, будет выглядеть так себе.

Кое-как прописал команды, вываливается и консольное сообщение, и инфо-сообщение. Проблема в другом.

Уже сейчас есть два вызова new MessageService. А теперь представим, что в заголовке того же MessageBox надо показывать имя и версию текущей сборки. Погнали? )))

Меняю конструктор для MessageService и моментом ловлю ошибки сборки. Что, в принципе, предсказуемо.

Теперь в инициализаторе аддона добавляю собственно контейнер из SimpleIbject:
```csharp
using SimpleInjector;
using Teigha.Runtime;

namespace NCad
{
    public class CadPlugin : IExtensionApplication
    {
        public void Initialize()
        {
            Container = new Container();
            ConfigureContainer(Container);
            Container.Verify();
        }

        public void Terminate()
        {
            Container?.Dispose();            
        }

        public static Container Container { get; private set; }

        private void ConfigureContainer(Container container)
        {

        }
    }
}
```
По-хорошему бы строки работы с контейнером надо заворачивать в try-catch, но пока и так сойдет.

```csharp
private void ConfigureContainer(Container container)
{
    container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

    container.Register<IMessageService>(() =>
    {
        IApplicationInfo applicationInfo = container.GetInstance<IApplicationInfo>();
        return new MessageService(applicationInfo);
    },
        Lifestyle.Singleton);
}
```

Singleton означает, что элемент будет инициализироваться только раз, при первом обращении к нему.

Теперь переделывать команды: вместо `IMessageService messageService = new MessageService();` ставлю `IMessageService messageService = CadPlugin.Container.GetInstance<IMessageService>();`, и все работает как заказано ;)

А теперь чисто по приколу - сервис по работе с текущим документом. Создал `IDocumentService` и его зарегистрировал в контейнере.

Ну и следом - две команды, с информацией по документу. Не проверял, поскольку пишу на машине, где никакого CADa не существует.

Теперь приступлю-ка к переделке реализации `IMessageService`. Текущая реализация не показывает в заголовке сообщения ни названия сборки, ни ее версии - ничего. Кроме того, подобная информация может понадобиться и для других окон. Кроме того, сообщения могут запросто "улетать" за окно наника, особенно в Linux. Так что проще будет дополнить `IApplicationInfo` нужными методами и свойствами.

Доработал, дополнил, запушил.

Если теперь в какой-то момент, к примеру, в IMessageService понадобится передавать не `IApplicationInfo`, а что-то другое, не понадобится бегать по всему коду. Достаточно будет изменить инициализацию приложения. Точнее, `ConfigureContainer()`.

===

# Если не копируются служебные библиотеки...

Особенно SimpleInjector. Нужно открыть csproj-файл основной сборки и добавить строку
```xml
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
```
Т.е. полный текст csproj в текущем варианте станет:
```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net6.0-windows</TargetFramework>
		<Nullable>enable</Nullable>
		<UseWPF>true</UseWPF>
		<UseWindowForms>true</UseWindowForms>
		<ImplicitUsings>enable</ImplicitUsings>
		<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
	</PropertyGroup>

	<ItemGroup>	
	  <PackageReference Include="nanoCAD.Platform.NET" Version="23.1.6324.4405" />
	  <PackageReference Include="SimpleInjector" Version="5.5.0" />
	</ItemGroup>

	<ItemGroup>
	  <ProjectReference Include="..\Abstractions\Abstractions.csproj" />
	</ItemGroup>

</Project>
```

# Если в работе инициализатора могут быть исключения

То слушаем доброго и умного дядю доктора и все, что было в инициализаторе, вытаскивается в отдельный (можно даже приватный) метод. Который уже и вызывать из `Initialize()`, оборачивая вызов в try-catch. Иначе отлов ошибок может и не сработать ;)