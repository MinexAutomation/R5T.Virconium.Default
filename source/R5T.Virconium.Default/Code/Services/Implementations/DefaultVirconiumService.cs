using System;
using System.IO;
using System.Linq;

using R5T.Code.VisualStudio.Model;
using R5T.Magyar.Extensions;

using R5T.T0064;

using ProjectFileUtilities = R5T.Code.VisualStudio.ProjectFile.Types.Utilities;


namespace R5T.Virconium.Default
{
    [ServiceImplementationMarker]
    public class DefaultVirconiumService : IVirconiumService, IServiceImplementation
    {
        public void AddMissingProjectDependencies(string solutionFilePath, TextWriter writer, bool dryRun = true)
        {
            // Write out input information.
            writer.WriteLine($"Target solution file path:\n{solutionFilePath}");

            // Get all projects in the solution.
            var solutionFile = SolutionFile.Load(solutionFilePath);

            var solutionProjectReferenceFilePaths = solutionFile.GetProjectReferenceFilePaths(solutionFilePath).ToList(); // Evaluate now.

            // Write out all solution projects.
            writer.WriteLine($"\nSolution projects: (count: {solutionProjectReferenceFilePaths.Count()}):");

            foreach (var solutionProjectReferenceFilePath in solutionProjectReferenceFilePaths.SortAlphabetically())
            {
                writer.WriteLine(solutionProjectReferenceFilePath);
            }

            // Get all dependencies of all projects, recursively.
            var projectReferenceDependencyFilePaths = ProjectFileUtilities.GetProjectReferenceDependencyFilePathsRecursive(solutionProjectReferenceFilePaths);

            // Write out all project reference dependency file paths.
            writer.WriteLine($"\nSolution project dependencies (count: {projectReferenceDependencyFilePaths.Count()}):");

            foreach (var projectReferenceDependencyFilePath in projectReferenceDependencyFilePaths.SortAlphabetically())
            {
                writer.WriteLine(projectReferenceDependencyFilePath);
            }

            // Get all project reference dependencies that must be added to the solution.
            var missingProjectReferenceDependencyFilePaths = projectReferenceDependencyFilePaths.Except(solutionProjectReferenceFilePaths);

            // Write out all missing project reference dependency file paths.
            writer.WriteLine($"\nMissing projects: (count: {missingProjectReferenceDependencyFilePaths.Count()}):");

            foreach (var missingProjectReferenceDependencyFilePath in missingProjectReferenceDependencyFilePaths.SortAlphabetically())
            {
                writer.WriteLine(missingProjectReferenceDependencyFilePath);
            }

            writer.WriteLine();

            // Add missing dependencies.
            if (dryRun)
            {
                writer.WriteLine("Dry run: no actions taken.");
            }
            else
            {
                writer.WriteLine("Adding missing dependencies...");

                foreach (var missingProjectReferenceDependencyFilePath in missingProjectReferenceDependencyFilePaths)
                {
                    solutionFile.AddProjectReferenceDependencyChecked(solutionFilePath, missingProjectReferenceDependencyFilePath);

                    writer.WriteLine($"Added '{missingProjectReferenceDependencyFilePath}'.");
                }

                solutionFile.Save(solutionFilePath);
            }
        }

        public void RemoveExtraneousDependencies(string solutionFilePath, TextWriter writer, bool dryRun = true)
        {
            var solutionFile = SolutionFile.Load(solutionFilePath);

            var projectFilePaths = solutionFile.GetNonDependencyProjectFilePaths(solutionFilePath).ToArray();

            // Write out input information.
            writer.WriteLine($"Target solution file path:\n{solutionFilePath}");

            foreach (var projectFilePath in projectFilePaths.OrderBy(x => x))
            {
                writer.WriteLine($"Target project file path:\n{projectFilePath}");
            }

            // Get all project reference dependency file paths.
            var allProjectReferenceDependencyFilePaths = ProjectFileUtilities.GetProjectReferenceDependencyFilePathsRecursive(projectFilePaths);

            // Remove any of the target project paths.
            var projectReferenceDependencyFilePaths = allProjectReferenceDependencyFilePaths.Except(projectFilePaths).ToList(); // Evaluate now.

            // Write out all project reference dependency file paths.
            writer.WriteLine($"\nTarget project dependencies (count: {projectReferenceDependencyFilePaths.Count()}):");

            foreach (var projectReferenceDependencyFilePath in projectReferenceDependencyFilePaths.SortAlphabetically())
            {
                writer.WriteLine(projectReferenceDependencyFilePath);
            }

            // Get all projects included in the solution.
            var solutionProjectReferenceDependencyFilePaths = solutionFile.GetProjectReferenceDependencyFilePaths(solutionFilePath).ToList(); // Evaluate now since enumerable will be changed later.

            // Write out all projects included in the solution.
            writer.WriteLine($"\nSolution projects: (count: {solutionProjectReferenceDependencyFilePaths.Count()}):");

            foreach (var solutionProjectReferenceDependencyFilePath in solutionProjectReferenceDependencyFilePaths.SortAlphabetically())
            {
                writer.WriteLine(solutionProjectReferenceDependencyFilePath);
            }

            // Get all extraneous dependency project references.
            var extraneousSolutionProjectReferenceDependencyFilePaths = solutionProjectReferenceDependencyFilePaths.Except(projectReferenceDependencyFilePaths);

            // List all extraneoius dependency project references.
            writer.WriteLine($"\nExtraneous solution projects (count: {extraneousSolutionProjectReferenceDependencyFilePaths.Count()}):");

            foreach (var extraneousSolutionProjectReferenceDependencyFilePath in extraneousSolutionProjectReferenceDependencyFilePaths.SortAlphabetically())
            {
                writer.WriteLine(extraneousSolutionProjectReferenceDependencyFilePath);
            }

            writer.WriteLine();

            // Remove extraneous dependencies.
            if (dryRun)
            {
                writer.WriteLine("Dry run: no actions taken.");
            }
            else
            {
                writer.WriteLine("Removing extraneous dependencies...");

                foreach (var extraneousDependency in extraneousSolutionProjectReferenceDependencyFilePaths.SortAlphabetically())
                {
                    solutionFile.RemoveProjectReference(solutionFilePath, extraneousDependency);

                    writer.WriteLine($"Removed '{extraneousDependency}'.");
                }

                solutionFile.Save(solutionFilePath);
            }
        }
    }
}
