using Models;

namespace FilesAPI.ViewModels.Mapper
{
	public static class ViewModelsMapper
	{
		public static FileDetailsViewModels ConvertFileDetailsToFileDetailsViewModels(FileDetails fileDetails)
		{
			return new FileDetailsViewModels
			{
				AddedBy = fileDetails.AddedBy,
				AddedDate = fileDetails.AddedDate,
				ContentType = fileDetails.ContentType,
				Description = fileDetails.Description,
				HashId = fileDetails.HashId,
				Id = fileDetails.Id,
				LastModified = fileDetails.LastModified,
				Name = fileDetails.Name,
				NumberOfDownloads = fileDetails.NumberOfDownloads,
				Size = fileDetails.Size,
				Tags = fileDetails.Tags
			};
		}
	}
}