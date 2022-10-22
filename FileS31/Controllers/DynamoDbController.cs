using DynamoDb.Libs.DynamoDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileS31.Controllers
{
    [Produces("application/json")]
    [Route("api/DynamoDb")]
    [ApiController]
    public class DynamoDbController : Controller
    {

        private readonly ICreateTable _createTable;
        private readonly IPutItem _putItem;
        private readonly IGetItem _getItem;
        private readonly IUpdateItem _updateItem;
        private readonly IDeleteTable _deleteTable;

        public DynamoDbController(ICreateTable createTable, IPutItem putItem, IGetItem getItem, IUpdateItem updateItem, IDeleteTable deleteTable)
        {
            _createTable = createTable;
            _putItem = putItem;
            _getItem = getItem;
            _updateItem = updateItem;
            _deleteTable = deleteTable;
        }

        #region POST

        [Route("CreateTable")]
        [HttpPost]
        public IActionResult CreateDynamoDbTable()
        {
            _createTable.CreateDynamoDbTable();

            return Ok();
        }

        #endregion

        #region PUT

        [Route("PutItems")]
        [HttpPut]
        public IActionResult PutItem([FromQuery] int id, string replyDateTime, double price)
        {
            _putItem.AddNewEntry(id, replyDateTime, price);

            return Ok();
        }

        [HttpPut]
        [Route("UpdateItem")]
        public async Task<IActionResult> UpdateItem([FromQuery] int id, double price)
        {
            var response = await _updateItem.Update(id, price);

            return Ok(response);
        }

        #endregion

        #region GET

        [Route("GetItems")]
        [HttpGet]
        public async Task<IActionResult> GetItems([FromQuery] int? id)
        {
            var response = await _getItem.GetItems(id);

            return Ok(response);
        }

        #endregion

        #region DELETE

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteTable([FromQuery] string tableName)
        {
            var response = await _deleteTable.ExecuteTableDelete(tableName);

            return Ok(response);
        }

        #endregion
    }
}
