using AndroidWebAPI.Models;
using AndroidWebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationRulesController : ControllerBase
    {
        private readonly INotificationRuleRepository _repository;

        public NotificationRulesController(INotificationRuleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rules = await _repository.GetAllAsync();

            return Ok(rules);
        }

        [HttpGet("{ruleId}")]
        public async Task<IActionResult> GetById(Guid ruleId)
        {
            var rule = await _repository.GetByIdAsync(ruleId);

            if (rule == null)
                return NotFound(new { message = "Notification rule not found." });

            return Ok(rule);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] NotificationRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.RuleName))
                return BadRequest(new { message = "Rule name is required." });

            if (string.IsNullOrWhiteSpace(rule.NotificationType))
                return BadRequest(new { message = "Notification type is required." });

            if (string.IsNullOrWhiteSpace(rule.TriggerType))
                return BadRequest(new { message = "Trigger type is required." });

            rule.RuleID = Guid.NewGuid();
            rule.CreatedAt = DateTime.Now;
            rule.UpdatedAt = null;

            var created = await _repository.CreateAsync(rule);

            return CreatedAtAction(
                nameof(GetById),
                new { ruleId = created.RuleID },
                created);
        }

        [HttpPut("{ruleId}")]
        public async Task<IActionResult> Update(
            Guid ruleId,
            [FromBody] NotificationRule rule)
        {
            var existing = await _repository.GetByIdAsync(ruleId);

            if (existing == null)
                return NotFound(new { message = "Notification rule not found." });

            if (string.IsNullOrWhiteSpace(rule.RuleName))
                return BadRequest(new { message = "Rule name is required." });

            if (string.IsNullOrWhiteSpace(rule.NotificationType))
                return BadRequest(new { message = "Notification type is required." });

            if (string.IsNullOrWhiteSpace(rule.TriggerType))
                return BadRequest(new { message = "Trigger type is required." });

            existing.RuleName = rule.RuleName;
            existing.NotificationType = rule.NotificationType;
            existing.TriggerType = rule.TriggerType;
            existing.TriggerValue = rule.TriggerValue;
            existing.InAppEnabled = rule.InAppEnabled;
            existing.SmsEnabled = rule.SmsEnabled;
            existing.EmailEnabled = rule.EmailEnabled;
            existing.IsEnabled = rule.IsEnabled;
            existing.UpdatedAt = DateTime.Now;

            var updated = await _repository.UpdateAsync(existing);

            return Ok(updated);
        }

        [HttpDelete("{ruleId}")]
        public async Task<IActionResult> Delete(Guid ruleId)
        {
            var deleted = await _repository.DeleteAsync(ruleId);

            if (!deleted)
                return NotFound(new { message = "Notification rule not found." });

            return Ok(new { message = "Notification rule deleted successfully." });
        }
    }
}