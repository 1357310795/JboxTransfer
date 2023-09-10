using System;

namespace JboxTransfer.Services.Contracts;

public interface IPageService
{
    Type GetPageType(string key);
}
