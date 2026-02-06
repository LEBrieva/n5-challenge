import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';

interface Props {
  children: React.ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
  }

  handleReload = () => {
    window.location.reload();
  };

  render() {
    if (this.state.hasError) {
      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '100vh',
            p: 3,
          }}
        >
          <Paper
            elevation={3}
            sx={{
              p: 4,
              maxWidth: 500,
              textAlign: 'center',
            }}
          >
            <ErrorOutlineIcon
              color="error"
              sx={{ fontSize: 80, mb: 2 }}
            />
            <Typography variant="h4" gutterBottom>
              Algo salió mal
            </Typography>
            <Typography variant="body1" color="text.secondary" paragraph>
              La aplicación encontró un error inesperado. Por favor, recarga la página.
            </Typography>
            {this.state.error && (
              <Typography
                variant="body2"
                color="error"
                sx={{
                  mt: 2,
                  p: 2,
                  bgcolor: 'error.light',
                  borderRadius: 1,
                  fontFamily: 'monospace',
                }}
              >
                {this.state.error.message}
              </Typography>
            )}
            <Button
              variant="contained"
              color="primary"
              onClick={this.handleReload}
              sx={{ mt: 3 }}
            >
              Recargar Página
            </Button>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}
