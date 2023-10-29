namespace Courses.API.APIModel
{
    public interface IAPIModel<TModel> where TModel : class
    {
        // will map data model to this model.
        void Map(TModel model);

        // will map this model to data model passed.
        void MapOut(TModel model);

        // will return new data model mapped from this.
        TModel Map();
    }
}
