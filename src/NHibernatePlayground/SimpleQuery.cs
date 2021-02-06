﻿namespace NHibernatePlayground
{
    using System.Collections.Generic;
    using System.Linq;

    using NHibernate;
    using NHibernate.Criterion;
    using NHibernate.Transform;

    using NHibernatePlayground.Model;

    public class SimpleQuery
    {
        private readonly ISession session;

        public SimpleQuery(ISession session)
        {
            this.session = session;
        }

        /// <param name="customerNameContains">
        /// specifies the characters the name of a customer should contain.
        /// </param>
        /// <remarks>
        /// This will result in the following SQL.
        /// <code language="sql">
        /// exec sp_executesql N'SELECT
        ///    this_.OrderID as y0_,
        ///    customeral1_.ContactName as y1_,
        ///    this_.OrderDate as y2_,
        ///    this_.ShippedDate as y3_,
        ///    this_.ShipAddress as y4_,
        ///    this_.ShipPostalCode as y5_,
        ///    this_.ShipCity as y6_,
        ///    this_.ShipCountry as y7_
        /// FROM
        ///    dbo.Orders this_
        /// inner join
        ///    dbo.Customers customeral1_ on this_.CustomerID=customeral1_.CustomerID
        /// WHERE
        ///     customeral1_.ContactName like @p0
        /// ORDER BY
        ///    y2_ desc
        /// OFFSET 0 ROWS FETCH FIRST @p0 ROWS ONLY',
        /// N'@p0 nvarchar(30),@p1 int',
        /// @p0=N'%{filter}%',
        /// @p1=20
        /// </code>
        /// </remarks>
        public IReadOnlyCollection<OrderItem> GetOrdersUsingQueryOver(string customerNameContains)
        {
            OrderItem orderItem = null;
            CustomerEntity customerAlias = null;

            var query = this.session
                .QueryOver<OrderEntity>()
                .JoinAlias(x => x.Customer, () => customerAlias)
                .SelectList(l => l
                    .Select(x => x.Id).WithAlias(() => orderItem.Id)
                    .Select(() => customerAlias.ContactName).WithAlias(() => orderItem.CustomerName)
                    .Select(x => x.OrderDate).WithAlias(() => orderItem.OrderDate)
                    .Select(x => x.ShippedDate).WithAlias(() => orderItem.ShippedDate)
                    .Select(x => x.ShipAddress).WithAlias(() => orderItem.Address)
                    .Select(x => x.ShipPostalCode).WithAlias(() => orderItem.PostCode)
                    .Select(x => x.ShipCity).WithAlias(() => orderItem.City)
                    .Select(x => x.ShipCountry).WithAlias(() => orderItem.Country))
                .WhereRestrictionOn(
                    () => customerAlias.ContactName).IsLike(customerNameContains, MatchMode.Anywhere)
                .OrderBy(x => x.OrderDate).Desc
                .TransformUsing(Transformers.AliasToBean<OrderItem>())
                .Take(20)
                .List<OrderItem>()
                .ToArray();

            return query;
        }

        /// <param name="customerNameContains">
        /// specifies the characters the name of a customer should contain.
        /// </param>
        /// <remarks>
        /// This will result in the following SQL.
        /// <code language="sql">
        /// exec sp_executesql N'select
        ///     orderentit0_.OrderID as col_0_0_,
        ///     customeren1_.ContactTitle as col_1_0_,
        ///     orderentit0_.OrderDate as col_2_0_,
        ///     orderentit0_.ShippedDate as col_3_0_,
        ///     orderentit0_.ShipAddress as col_4_0_,
        ///     orderentit0_.ShipPostalCode as col_5_0_,
        ///     orderentit0_.ShipCity as col_6_0_,
        ///     orderentit0_.ShipCountry as col_7_0_
        /// from
        ///     dbo.Orders orderentit0_
        /// inner join
        ///     dbo.Customers customeren1_ on orderentit0_.CustomerID=customeren1_.CustomerID
        /// where
        ///     customeren1_.ContactName like @p0
        /// ORDER BY
        ///     orderentit0_.OrderDate DESC
        /// OFFSET 0 ROWS FETCH FIRST @p1 ROWS ONLY',
        /// N'@p0 nvarchar(30),@p1 int',
        /// @p0=N'%{filter}%',
        /// @p1=20
        /// </code>
        /// </remarks>
        public IReadOnlyCollection<OrderItem> GetOrdersUsingHql(string customerNameContains)
        {
            const string hqlQuery = @"
                SELECT
                    o.Id                AS Id,
                    c.ContactName       AS CustomerName,
                    o.OrderDate         AS OrderDate,
                    o.ShippedDate       AS ShippedDate,
                    o.ShipAddress       AS Address,
                    o.ShipPostalCode    AS PostCode,
                    o.ShipCity          AS City,
                    o.ShipCountry       AS Country
                FROM
                    OrderEntity AS o
                INNER JOIN
                    o.Customer AS c
                WHERE
                    c.ContactName LIKE :customerNameFilter
                ORDER BY
                    o.OrderDate DESC";

            var query = this.session
                .CreateQuery(hqlQuery)
                .SetParameter("customerNameFilter", "%" + customerNameContains + "%")
                .SetMaxResults(20)
                .SetResultTransformer(Transformers.AliasToBean<OrderItem>())
                .List<OrderItem>()
                .ToArray();

            return query;
        }
    }
}