using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example
{

    public class TestData : Common.Data.Orm.OrmBase<TestData>
    {


        public object ID{set;get;}

        public string Name { set; get; }

        public string Value { set; get; }

        public string Query_Id { set; get; }


        public TestData() { }

    }



    public class TestData1 : Common.Data.Orm.OrmBase<TestData1>
    {

        public object ID { set; get; }

        public string Name { set; get; }

        public string Value { set; get; }

        public string Query_Id { set; get; }


        public TestData1() { }

    }


  
}
