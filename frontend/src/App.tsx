import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline, AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import Dashboard from './pages/Dashboard';
import './App.css';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <Box sx={{ flexGrow: 1 }}>
          <AppBar position="static">
            <Toolbar>
              <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
                Investment Portfolio Manager
              </Typography>
              <Button color="inherit" href="/">
                Dashboard
              </Button>
              <Button color="inherit" href="/portfolios">
                Portfolios
              </Button>
              <Button color="inherit" href="/securities">
                Securities
              </Button>
              <Button color="inherit" href="/transactions">
                Transactions
              </Button>
            </Toolbar>
          </AppBar>
          
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/portfolios" element={<div>Portfolios Page Coming Soon</div>} />
            <Route path="/securities" element={<div>Securities Page Coming Soon</div>} />
            <Route path="/transactions" element={<div>Transactions Page Coming Soon</div>} />
          </Routes>
        </Box>
      </Router>
    </ThemeProvider>
  );
};

export default App;
