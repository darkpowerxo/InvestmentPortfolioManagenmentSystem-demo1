import React from 'react';
import {
  Box,
  Container,
  Paper,
  Typography,
  Avatar,
  Card,
  CardContent,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Stack,
} from '@mui/material';
import {
  Work as WorkIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  Business as BusinessIcon,
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';

export const UserProfile: React.FC = () => {
  const { user } = useAuth();

  if (!user) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Typography variant="h4" color="error">
          User information not available. Please refresh the page.
        </Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom fontWeight="bold">
        User Profile
      </Typography>

      <Stack spacing={3}>
        {/* Profile Information */}
        <Paper elevation={2} sx={{ p: 3 }}>
          <Typography variant="h6" fontWeight="bold" gutterBottom>
            Personal Information
          </Typography>

          {/* Avatar and Basic Info */}
          <Box display="flex" alignItems="center" gap={2} mb={3}>
            <Avatar
              sx={{ 
                width: 80, 
                height: 80, 
                bgcolor: 'primary.main',
                fontSize: '2rem',
                fontWeight: 'bold'
              }}
            >
              {user.firstName.charAt(0)}{user.lastName.charAt(0)}
            </Avatar>
            <Box>
              <Typography variant="h6" fontWeight="bold">
                {user.firstName} {user.lastName}
              </Typography>
              <Chip 
                label={user.role} 
                color="primary" 
                size="small" 
                sx={{ textTransform: 'capitalize' }}
              />
            </Box>
          </Box>

          {/* User Details */}
          <List>
            <ListItem>
              <ListItemIcon>
                <EmailIcon color="action" />
              </ListItemIcon>
              <ListItemText
                primary="Email"
                secondary={user.email}
              />
            </ListItem>
            {user.phoneNumber && (
              <ListItem>
                <ListItemIcon>
                  <PhoneIcon color="action" />
                </ListItemIcon>
                <ListItemText
                  primary="Phone"
                  secondary={user.phoneNumber}
                />
              </ListItem>
            )}
            {user.department && (
              <ListItem>
                <ListItemIcon>
                  <BusinessIcon color="action" />
                </ListItemIcon>
                <ListItemText
                  primary="Department"
                  secondary={user.department}
                />
              </ListItem>
            )}
            <ListItem>
              <ListItemIcon>
                <WorkIcon color="action" />
              </ListItemIcon>
              <ListItemText
                primary="Role"
                secondary={user.role}
              />
            </ListItem>
          </List>
        </Paper>

        {/* Account Information */}
        <Card elevation={2}>
          <CardContent>
            <Typography variant="h6" fontWeight="bold" gutterBottom>
              Account Information
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Profile management features are coming soon. You can view your current 
              information above. For any changes, please contact your administrator.
            </Typography>
          </CardContent>
        </Card>
      </Stack>
    </Container>
  );
};

export default UserProfile;