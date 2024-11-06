namespace MeterApi.Controllers;

public interface IAccountsCache
{
    bool Contains(int accountId);
    int Refresh();
}

public class AccountsCache : IAccountsCache
{
    private readonly AppDbContext _dbContext;
    private ILogger<AccountsCache> _logger;
    private HashSet<int> _accountIds;
    
    public AccountsCache(AppDbContext dbContext, ILogger<AccountsCache> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _accountIds = [];
    }
    
    public bool Contains(int accountId)
    {
        if (_accountIds == null || _accountIds.Count == 0)
        {
            Refresh();
        }
        
        return _accountIds?.Contains(accountId) ?? false;
    }
    
    public int Refresh()
    {
        _accountIds = _dbContext.Accounts.Select(a => a.Id).ToHashSet();
        return _accountIds.Count;
    }
}