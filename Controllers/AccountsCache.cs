namespace MeterApi.Controllers;

public class AccountsCache
{
    private readonly AppDbContext _dbContext;
    private ILogger<AccountsCache> _logger;
    private HashSet<int> _accountIds;

    public AccountsCache()
    {
        
    }
    
    public AccountsCache(AppDbContext dbContext, ILogger<AccountsCache> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _accountIds = new HashSet<int>();
    }
    
    public virtual bool Contains(int accountId)
    {
        if (_accountIds == null || _accountIds.Count == 0)
        {
            Refresh();
        }
        
        return _accountIds.Contains(accountId);
    }
    
    public int Refresh()
    {
        _accountIds = _dbContext.Accounts.Select(a => a.Id).ToHashSet();
        return _accountIds.Count;
    }
}