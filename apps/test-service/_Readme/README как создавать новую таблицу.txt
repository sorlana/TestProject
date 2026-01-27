
если добавляется новая таблица

прописать таблицу в стартапе, без этого работать не будет
прописать таблицу в DatabaseContext, без этого работать не будет
прописать репозиторий в Register, без этого работать не будет

---------------------------------

если в таблицу добавляются/удаляются новые поля, то они прописываются в трех моделях

WebApi/Models/fileModel.cs
Domain.Entities/file.cs
Services.Contracts/file/fileDTO.cs

а также в файле
Services.Implementations/Mapping/fileMappingProfile.cs

сделать миграции add и update

если последняя миграция ошибочна, ее можно удалить командой update to migration и remove

---------------------------------






