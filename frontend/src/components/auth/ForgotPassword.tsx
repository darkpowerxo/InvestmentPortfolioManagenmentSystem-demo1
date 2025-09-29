import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Box,
  Button,
  TextField,
  Typography,
  Paper,
  Container,
  Alert,
  CircularProgress,
  InputAdornment,
} from '@mui/material';
import {
  Email as EmailIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material';
import { usePasswordReset } from '../../hooks';

export const ForgotPassword: React.FC = () => {
  const [email, setEmail] = useState('');
  const [emailError, setEmailError] = useState('');
  const { requestReset, isLoading, error, success } = usePasswordReset();

  const validateEmail = (email: string): boolean => {
    if (!email.trim()) {
      setEmailError('Email is required');
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      setEmailError('Please enter a valid email address');
      return false;
    }

    setEmailError('');
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateEmail(email)) {
      return;
    }

    await requestReset({ email: email.trim() });
  };

  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setEmail(value);
    
    // Clear error when user starts typing
    if (emailError && value.trim()) {
      setEmailError('');
    }
  };

  if (success) {
    return (
      <Container component="main" maxWidth="sm">
        <Box
          sx={{
            minHeight: '100vh',
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            py: 4,
          }}
        >
          <Paper
            elevation={8}
            sx={{
              p: 4,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              textAlign: 'center',
            }}
          >
            {/* Success Icon */}
            <Box
              sx={{
                width: 80,
                height: 80,
                borderRadius: '50%',
                backgroundColor: 'success.light',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mb: 3,
              }}
            >
              <EmailIcon sx={{ fontSize: 40, color: 'success.main' }} />
            </Box>

            {/* Success Message */}
            <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
              Check Your Email
            </Typography>
            
            <Typography variant="body1" color="text.secondary" paragraph>
              We've sent a password reset link to:
            </Typography>
            
            <Typography variant="body1" fontWeight="medium" paragraph>
              {email}
            </Typography>
            
            <Typography variant="body2" color="text.secondary" paragraph>
              Click the link in the email to reset your password. If you don't receive 
              an email within a few minutes, check your spam folder or try again.
            </Typography>

            {/* Actions */}
            <Box sx={{ width: '100%', mt: 3 }}>
              <Button
                component={Link}
                to="/login"
                fullWidth
                variant="contained"
                size="large"
                sx={{ mb: 2 }}
              >
                Back to Sign In
              </Button>
              
              <Button
                onClick={() => window.location.reload()}
                fullWidth
                variant="outlined"
                size="large"
              >
                Try Another Email
              </Button>
            </Box>
          </Paper>
        </Box>
      </Container>
    );
  }

  return (
    <Container component="main" maxWidth="sm">
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          py: 4,
        }}
      >
        <Paper
          elevation={8}
          sx={{
            p: 4,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
          }}
        >
          {/* Header */}
          <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
            Forgot Password?
          </Typography>
          
          <Typography 
            variant="body1" 
            color="text.secondary" 
            textAlign="center"
            paragraph
          >
            Enter your email address and we'll send you a link to reset your password.
          </Typography>

          {/* Error Alert */}
          {error && (
            <Alert severity="error" sx={{ width: '100%', mb: 3 }}>
              {error}
            </Alert>
          )}

          {/* Form */}
          <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
            <TextField
              fullWidth
              name="email"
              type="email"
              label="Email Address"
              value={email}
              onChange={handleEmailChange}
              error={!!emailError}
              helperText={emailError}
              disabled={isLoading}
              autoComplete="email"
              autoFocus
              sx={{ mb: 3 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <EmailIcon color="action" />
                  </InputAdornment>
                ),
              }}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={isLoading || !email.trim()}
              sx={{ 
                mb: 3,
                py: 1.5,
                fontSize: '1rem',
                fontWeight: 'bold'
              }}
            >
              {isLoading ? (
                <>
                  <CircularProgress size={20} sx={{ mr: 1 }} />
                  Sending Reset Link...
                </>
              ) : (
                'Send Reset Link'
              )}
            </Button>

            {/* Back to Login */}
            <Button
              component={Link}
              to="/login"
              fullWidth
              variant="outlined"
              size="large"
              disabled={isLoading}
              startIcon={<ArrowBackIcon />}
              sx={{ 
                py: 1.5,
                fontSize: '1rem'
              }}
            >
              Back to Sign In
            </Button>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default ForgotPassword;