# HURO Frontend

This is an [Expo](https://expo.dev) project with deployment capabilities to mobile and web.

## Environment Setup:

1. **Install dependencies**

   ```bash
   npm install
   ```

2. **Add Firebase API key to `frontend/firebase/config.ts`**
3. **Start the app**

   ```bash
    npx expo start
   ```

4. **Open web view:**

   Press `w`


### Deploy Website
1. Run `npx expo export -p web` or `npm run deploy`

2. Upload dist folder to Netlify (currently only Nikita can do this because of Netlify free plan.)

3. View deployed website [here](https://hurovr.netlify.app/).
   

### Testing
1. Add a test in `__tests__` folder
2. Run all tests with `npm run test`


### CI/CD Workflow
This project includes automated workflows using GitHub Actions:
- **Build & Test:** Runs on every push and pull request.
- **Security Scans:** Automated vulnerability scanning using CodeQL and Trivy.
- **Docker Build & Push:** Builds and pushes Docker images to DockerHub.
- **Automated README Updates:** Fetches project stats and updates `README.md` daily.

### Environment Variables
Ensure the following environment variables are set in GitHub Secrets (`Settings > Secrets and variables > Actions`):

| Secret Name       | Description |
|------------------|-------------|
| `AWS_ACCESS_KEY_ID` | AWS access key for S3 deployments |
| `AWS_SECRET_ACCESS_KEY` | AWS secret key |
| `DOCKER_USERNAME` | DockerHub username |
| `DOCKER_PASSWORD` | DockerHub access token |
| `GH_TOKEN` | GitHub API token for fetching repository data |

### Local Development & CI/CD Testing
To test GitHub Actions locally, install `act`:
```bash
brew install act  # macOS
sudo apt install act  # Linux
choco install act-cli  # Windows
```
Run workflows locally:
```bash
act -W .github/workflows/ci-cd.yml
```
To debug a specific workflow:
```bash
act -W .github/workflows/update-readme.yml -v
```

This project uses [file-based routing](https://docs.expo.dev/router/introduction).

