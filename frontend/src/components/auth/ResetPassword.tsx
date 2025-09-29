import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
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
  IconButton,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material';
import {
  Lock as LockIcon,
  Visibility,
  VisibilityOff,
  Check as CheckIcon,
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material';
import { usePasswordReset } from '../../hooks';

interface PasswordValidation {
  minLength: boolean;
  hasUppercase: boolean;
  hasLowercase: boolean;
  hasNumber: boolean;
  hasSpecialChar: boolean;
}

export const ResetPassword: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [validationErrors, setValidationErrors] = useState<{
    password?: string;
    confirmPassword?: string;
  }>({});
  
  const { confirmReset, isLoading, error, success } = usePasswordReset();
  
  // Get token from URL parameters
  const token = searchParams.get('token');
  const email = searchParams.get('email');

  // Password validation rules
  const validatePassword = (password: string): PasswordValidation => {
    return {
      minLength: password.length >= 8,
      hasUppercase: /[A-Z]/.test(password),
      hasLowercase: /[a-z]/.test(password),
      hasNumber: /\d/.test(password),
      hasSpecialChar: /[!@#$%^&*(),.?":{}|<>]/.test(password),
    };
  };

  const passwordValidation = validatePassword(password);
  const isPasswordValid = Object.values(passwordValidation).every(Boolean);

  // Check if token is present on component mount
  useEffect(() => {
    if (!token || !email) {
      navigate('/forgot-password', { 
        state: { 
          message: 'Invalid or missing reset token. Please request a new password reset link.' 
        } 
      });
    }
  }, [token, email, navigate]);

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setPassword(value);
    
    // Clear password error when user starts typing
    if (validationErrors.password) {
      setValidationErrors(prev => ({ ...prev, password: undefined }));
    }
  };

  const handleConfirmPasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setConfirmPassword(value);
    
    // Clear confirm password error when user starts typing
    if (validationErrors.confirmPassword) {
      setValidationErrors(prev => ({ ...prev, confirmPassword: undefined }));
    }
  };

  const validateForm = (): boolean => {
    const errors: { password?: string; confirmPassword?: string } = {};

    if (!password) {
      errors.password = 'Password is required';
    } else if (!isPasswordValid) {
      errors.password = 'Password does not meet requirements';
    }

    if (!confirmPassword) {
      errors.confirmPassword = 'Please confirm your password';
    } else if (password !== confirmPassword) {
      errors.confirmPassword = 'Passwords do not match';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm() || !token || !email) {
      return;
    }

    const resetSuccess = await confirmReset({
      token,
      newPassword: password,
      confirmPassword: confirmPassword,
    });

    if (resetSuccess) {
      // Redirect to login after successful reset
      setTimeout(() => {
        navigate('/login', {
          state: {
            message: 'Password reset successful! Please sign in with your new password.',
          },
        });
      }, 2000);
    }
  };

  const togglePasswordVisibility = (field: 'password' | 'confirmPassword') => {
    if (field === 'password') {
      setShowPassword(prev => !prev);
    } else {
      setShowConfirmPassword(prev => !prev);
    }
  };

  // Show success message
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
              <CheckIcon sx={{ fontSize: 40, color: 'success.main' }} />
            </Box>

            {/* Success Message */}
            <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
              Password Reset Successful!
            </Typography>
            
            <Typography variant="body1" color="text.secondary" paragraph>
              Your password has been successfully reset. You will be redirected to the 
              sign-in page in a few seconds.
            </Typography>

            <CircularProgress size={24} sx={{ mt: 2 }} />
          </Paper>
        </Box>
      </Container>
    );
  }

  // Don't render if no token (will redirect)
  if (!token || !email) {
    return null;
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
            Reset Password
          </Typography>
          
          <Typography 
            variant="body1" 
            color="text.secondary" 
            textAlign="center"
            paragraph
          >
            Enter your new password below.
          </Typography>

          {/* Error Alert */}
          {error && (
            <Alert severity="error" sx={{ width: '100%', mb: 3 }}>
              {error}
            </Alert>
          )}

          {/* Form */}
          <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
            {/* New Password */}
            <TextField
              fullWidth
              name="password"
              type={showPassword ? 'text' : 'password'}
              label="New Password"
              value={password}
              onChange={handlePasswordChange}
              error={!!validationErrors.password}
              helperText={validationErrors.password}
              disabled={isLoading}
              autoComplete="new-password"
              sx={{ mb: 2 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <LockIcon color="action" />
                  </InputAdornment>
                ),
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      aria-label="toggle password visibility"
                      onClick={() => togglePasswordVisibility('password')}
                      edge="end"
                      disabled={isLoading}
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            {/* Password Requirements */}
            {password && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Password requirements:
                </Typography>
                <List dense sx={{ py: 0 }}>
                  <ListItem sx={{ py: 0, px: 1 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <CheckIcon
                        fontSize="small"
                        color={passwordValidation.minLength ? 'success' : 'disabled'}
                      />
                    </ListItemIcon>
                    <ListItemText
                      primary="At least 8 characters"
                      sx={{
                        '& .MuiListItemText-primary': {
                          fontSize: '0.875rem',
                          color: passwordValidation.minLength ? 'success.main' : 'text.disabled',
                        },
                      }}
                    />
                  </ListItem>
                  <ListItem sx={{ py: 0, px: 1 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <CheckIcon
                        fontSize="small"
                        color={passwordValidation.hasUppercase ? 'success' : 'disabled'}
                      />
                    </ListItemIcon>
                    <ListItemText
                      primary="One uppercase letter"
                      sx={{
                        '& .MuiListItemText-primary': {
                          fontSize: '0.875rem',
                          color: passwordValidation.hasUppercase ? 'success.main' : 'text.disabled',
                        },
                      }}
                    />
                  </ListItem>
                  <ListItem sx={{ py: 0, px: 1 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <CheckIcon
                        fontSize="small"
                        color={passwordValidation.hasLowercase ? 'success' : 'disabled'}
                      />
                    </ListItemIcon>
                    <ListItemText
                      primary="One lowercase letter"
                      sx={{
                        '& .MuiListItemText-primary': {
                          fontSize: '0.875rem',
                          color: passwordValidation.hasLowercase ? 'success.main' : 'text.disabled',
                        },
                      }}
                    />
                  </ListItem>
                  <ListItem sx={{ py: 0, px: 1 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <CheckIcon
                        fontSize="small"
                        color={passwordValidation.hasNumber ? 'success' : 'disabled'}
                      />
                    </ListItemIcon>
                    <ListItemText
                      primary="One number"
                      sx={{
                        '& .MuiListItemText-primary': {
                          fontSize: '0.875rem',
                          color: passwordValidation.hasNumber ? 'success.main' : 'text.disabled',
                        },
                      }}
                    />
                  </ListItem>
                  <ListItem sx={{ py: 0, px: 1 }}>
                    <ListItemIcon sx={{ minWidth: 32 }}>
                      <CheckIcon
                        fontSize="small"
                        color={passwordValidation.hasSpecialChar ? 'success' : 'disabled'}
                      />
                    </ListItemIcon>
                    <ListItemText
                      primary="One special character"
                      sx={{
                        '& .MuiListItemText-primary': {
                          fontSize: '0.875rem',
                          color: passwordValidation.hasSpecialChar ? 'success.main' : 'text.disabled',
                        },
                      }}
                    />
                  </ListItem>
                </List>
              </Box>
            )}

            {/* Confirm Password */}
            <TextField
              fullWidth
              name="confirmPassword"
              type={showConfirmPassword ? 'text' : 'password'}
              label="Confirm New Password"
              value={confirmPassword}
              onChange={handleConfirmPasswordChange}
              error={!!validationErrors.confirmPassword}
              helperText={validationErrors.confirmPassword}
              disabled={isLoading}
              autoComplete="new-password"
              sx={{ mb: 3 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <LockIcon color="action" />
                  </InputAdornment>
                ),
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      aria-label="toggle confirm password visibility"
                      onClick={() => togglePasswordVisibility('confirmPassword')}
                      edge="end"
                      disabled={isLoading}
                    >
                      {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={isLoading || !isPasswordValid || password !== confirmPassword}
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
                  Resetting Password...
                </>
              ) : (
                'Reset Password'
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

export default ResetPassword;