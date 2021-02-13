﻿namespace MoreCraftables.Framework.API
{
    public interface IMoreCraftablesAPI
    {
        void AddHandledType(IHandledType handledType);
        void AddObjectFactory(IObjectFactory objectFactory);
    }
}