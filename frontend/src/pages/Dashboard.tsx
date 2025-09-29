import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Paper,
  Chip,
  Button,
  Alert,
  CircularProgress,
  Container,
} from '@mui/material';
import {
  TrendingUp,
  TrendingDown,
  AccountBalance,
  ShowChart,
  Assessment,
  Warning
} from '@mui/icons-material';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { usePortfolios, useTopPositions, useRecentTransactions } from '../hooks';
import { formatCurrency, formatPercentage, getGainLossColor, generateColors } from '../utils/formatting';
import { Portfolio, Position, Transaction } from '../types';

interface DashboardCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: React.ReactNode;
  color?: string;
  trend?: number;
}

const DashboardCard: React.FC<DashboardCardProps> = ({ title, value, subtitle, icon, color = '#2196f3', trend }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box display="flex" alignItems="center" justifyContent="space-between">
        <Box>
          <Typography color="textSecondary" gutterBottom variant="h6">
            {title}
          </Typography>
          <Typography variant="h4" color={color}>
            {typeof value === 'number' ? formatCurrency(value) : value}
          </Typography>
          {subtitle && (
            <Typography color="textSecondary" variant="body2">
              {subtitle}
            </Typography>
          )}
          {trend !== undefined && (
            <Box display="flex" alignItems="center" mt={1}>
              {trend >= 0 ? (
                <TrendingUp sx={{ color: '#4caf50', mr: 0.5 }} />
              ) : (
                <TrendingDown sx={{ color: '#f44336', mr: 0.5 }} />
              )}
              <Typography 
                variant="body2" 
                sx={{ color: trend >= 0 ? '#4caf50' : '#f44336' }}
              >
                {formatPercentage(Math.abs(trend))}
              </Typography>
            </Box>
          )}
        </Box>
        <Box sx={{ color, fontSize: 48 }}>
          {icon}
        </Box>
      </Box>
    </CardContent>
  </Card>
);

interface PortfolioAllocationChartProps {
  portfolios: Portfolio[];
}

const PortfolioAllocationChart: React.FC<PortfolioAllocationChartProps> = ({ portfolios }) => {
  const data = portfolios.map((portfolio, index) => ({
    name: portfolio.name,
    value: portfolio.currentValue,
    fill: generateColors(portfolios.length)[index]
  }));

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Portfolio Allocation
        </Typography>
        <ResponsiveContainer width="100%" height={300}>
          <PieChart>
            <Pie
              data={data}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={(entry: any) => `${entry.name} ${(entry.percent * 100).toFixed(0)}%`}
              outerRadius={80}
              fill="#8884d8"
              dataKey="value"
            >
              {data.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.fill} />
              ))}
            </Pie>
            <Tooltip formatter={(value) => formatCurrency(value as number)} />
          </PieChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
};

interface TopPositionsListProps {
  positions: Position[];
}

const TopPositionsList: React.FC<TopPositionsListProps> = ({ positions }) => (
  <Card>
    <CardContent>
      <Typography variant="h6" gutterBottom>
        Top Positions
      </Typography>
      {positions.slice(0, 5).map((position) => (
        <Box key={position.id} display="flex" justifyContent="space-between" alignItems="center" py={1}>
          <Box>
            <Typography variant="body1" fontWeight="bold">
              {position.security?.symbol || 'N/A'}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              {position.security?.name || 'Unknown Security'}
            </Typography>
          </Box>
          <Box textAlign="right">
            <Typography variant="body1">
              {formatCurrency(position.marketValue)}
            </Typography>
            <Typography 
              variant="body2" 
              sx={{ color: getGainLossColor(position.unrealizedGainLoss) }}
            >
              {formatPercentage(position.unrealizedGainLossPercent)}
            </Typography>
          </Box>
        </Box>
      ))}
    </CardContent>
  </Card>
);

interface RecentTransactionsListProps {
  transactions: Transaction[];
}

const RecentTransactionsList: React.FC<RecentTransactionsListProps> = ({ transactions }) => (
  <Card>
    <CardContent>
      <Typography variant="h6" gutterBottom>
        Recent Transactions
      </Typography>
      {transactions.slice(0, 5).map((transaction) => (
        <Box key={transaction.id} display="flex" justifyContent="space-between" alignItems="center" py={1}>
          <Box>
            <Typography variant="body1" fontWeight="bold">
              {transaction.security?.symbol || 'N/A'}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              {transaction.type} {transaction.quantity} shares
            </Typography>
          </Box>
          <Box textAlign="right">
            <Typography variant="body1">
              {formatCurrency(transaction.netAmount)}
            </Typography>
            <Chip 
              label={transaction.type}
              size="small"
              color={transaction.type === 'Buy' ? 'primary' : 'secondary'}
            />
          </Box>
        </Box>
      ))}
    </CardContent>
  </Card>
);

const Dashboard: React.FC = () => {
  const { data: portfoliosData, loading: portfoliosLoading, error: portfoliosError } = usePortfolios();
  const { data: topPositions, loading: positionsLoading, error: positionsError } = useTopPositions(10);
  const { data: recentTransactions, loading: transactionsLoading, error: transactionsError } = useRecentTransactions(30);

  const portfolios = portfoliosData?.items || [];
  const positions = topPositions || [];
  const transactions = recentTransactions || [];

  // Calculate summary statistics
  const totalPortfolioValue = portfolios.reduce((sum, p) => sum + p.currentValue, 0);
  const totalDayChange = portfolios.reduce((sum, p) => sum + (p.currentValue * 0.005), 0); // Mock day change
  const totalPositions = positions.length;
  const totalCash = portfolios.reduce((sum, p) => sum + p.cashBalance, 0);

  const isLoading = portfoliosLoading || positionsLoading || transactionsLoading;
  const hasError = portfoliosError || positionsError || transactionsError;

  if (isLoading) {
    return (
      <Container maxWidth="xl">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (hasError) {
    return (
      <Container maxWidth="xl">
        <Alert severity="error" sx={{ mt: 2 }}>
          {portfoliosError || positionsError || transactionsError}
        </Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl">
      <Box sx={{ flexGrow: 1, py: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Investment Dashboard
        </Typography>
        
        {/* Summary Cards */}
        <Box display="flex" gap={3} flexWrap="wrap" sx={{ mb: 3 }}>
          <Box flex="1 1 300px">
            <DashboardCard
              title="Total Portfolio Value"
              value={totalPortfolioValue}
              icon={<AccountBalance />}
              color="#2196f3"
              trend={0.75} // Mock trend
            />
          </Box>
          <Box flex="1 1 300px">
            <DashboardCard
              title="Today's Change"
              value={formatCurrency(totalDayChange)}
              subtitle={formatPercentage(0.75)}
              icon={<TrendingUp />}
              color={getGainLossColor(totalDayChange)}
            />
          </Box>
          <Box flex="1 1 300px">
            <DashboardCard
              title="Total Positions"
              value={totalPositions.toString()}
              subtitle="Active positions"
              icon={<ShowChart />}
              color="#4caf50"
            />
          </Box>
          <Box flex="1 1 300px">
            <DashboardCard
              title="Cash Balance"
              value={totalCash}
              subtitle="Available liquidity"
              icon={<Assessment />}
              color="#ff9800"
            />
          </Box>
        </Box>

        {/* Charts and Lists */}
        <Box display="flex" gap={3} flexWrap="wrap">
          <Box flex="1 1 500px">
            <PortfolioAllocationChart portfolios={portfolios} />
          </Box>
          <Box flex="1 1 500px">
            <TopPositionsList positions={positions} />
          </Box>
          <Box flex="1 1 500px" sx={{ mt: 3 }}>
            <RecentTransactionsList transactions={transactions} />
          </Box>
          <Box flex="1 1 500px" sx={{ mt: 3 }}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Portfolio Performance
                </Typography>
                <Box display="flex" justifyContent="center" alignItems="center" height="200px">
                  <Typography color="textSecondary">
                    Performance chart coming soon...
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Box>
        </Box>

        {/* Quick Actions */}
        <Paper sx={{ p: 2, mt: 3 }}>
          <Typography variant="h6" gutterBottom>
            Quick Actions
          </Typography>
          <Box display="flex" gap={2} flexWrap="wrap">
            <Button variant="contained" color="primary">
              New Portfolio
            </Button>
            <Button variant="outlined" color="primary">
              Add Transaction
            </Button>
            <Button variant="outlined" color="secondary">
              Run Risk Analysis
            </Button>
            <Button variant="outlined" color="info">
              Generate Report
            </Button>
          </Box>
        </Paper>

        {/* Alerts */}
        {portfolios.some(p => p.cashBalance < 10000) && (
          <Alert severity="warning" icon={<Warning />} sx={{ mt: 2 }}>
            Some portfolios have low cash balances. Consider rebalancing or adding liquidity.
          </Alert>
        )}
      </Box>
    </Container>
  );
};

export default Dashboard;