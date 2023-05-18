using AutoMapper;
using VRGardenAlpha.Data;
using VRGardenAlpha.Models;

namespace VRGardenAlpha
{
    public class VRClassMap : Profile
    {
        public VRClassMap()
        {
            CreateMap<Post, PostModel>();   
            CreateMap<Post, SearchablePost>(); 
        }
    }
}