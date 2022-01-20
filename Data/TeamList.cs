using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class TeamList :  List<Team>
    {
        protected Team? GetByID(Guid id)
        {
            return (from team in this
                    where team.ID == id
                    select team).SingleOrDefault();
        }

        protected Team? GetByName(string name)
        {
            return (from team in this
                    where String.Equals(team.Name, name, StringComparison.InvariantCultureIgnoreCase)
                    select team).SingleOrDefault();
        }

        public Team? this[Guid id] => this.GetByID(id);

        public Team? this[string name] => this.GetByName(name);
    }
}
