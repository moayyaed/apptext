﻿using AppText.Core.Shared.Commands;
using AppText.Core.Storage;
using System.Threading.Tasks;

namespace AppText.Core.ContentManagement
{
    public class SaveContentCollectionCommand : ICommand
    {
        public ContentCollection ContentCollection { get; }

        public SaveContentCollectionCommand(ContentCollection contentCollection)
        {
            this.ContentCollection = contentCollection;
        }
    }

    public class SaveContentCollectionCommandHandler : ICommandHandler<SaveContentCollectionCommand>
    {
        private readonly IContentStore _contentStore;
        private readonly ContentCollectionValidator _validator;
        private readonly IVersioner _versioner;

        public SaveContentCollectionCommandHandler(IContentStore contentStore, ContentCollectionValidator validator, IVersioner versioner)
        {
            _contentStore = contentStore;
            _validator = validator;
            _versioner = versioner;
        }

        public async Task<CommandResult> Handle(SaveContentCollectionCommand command)
        {
            var result = new CommandResult();

            if (! await _validator.IsValid(command.ContentCollection))
            {
                result.AddValidationErrors(_validator.Errors);
            }
            else
            {
                if (! await _versioner.SetVersion(command.ContentCollection))
                {
                    result.SetVersionError();
                }
                else
                {
                    if (command.ContentCollection.Id == null)
                    {
                        await _contentStore.AddContentCollection(command.ContentCollection);
                    }
                    else
                    {
                        await _contentStore.UpdateContentCollection(command.ContentCollection);
                    }
                }
            }
            return result;
        }
    }
}
