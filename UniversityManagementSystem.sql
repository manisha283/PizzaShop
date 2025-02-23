-- Students Table
CREATE TABLE Students (
    StudentID SERIAL PRIMARY KEY,
    StudentName VARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender VARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
    EnrollmentDate DATE NOT NULL,
    Major VARCHAR(100) NOT NULL
);

-- Courses Table
CREATE TABLE Courses (
    CourseID SERIAL PRIMARY KEY,
    CourseName VARCHAR(100) NOT NULL,
    Department VARCHAR(100) NOT NULL,
    Credits INT CHECK (Credits > 0)
);

-- Professors Table
CREATE TABLE Professors (
    ProfessorID SERIAL PRIMARY KEY,
    ProfessorName VARCHAR(100) NOT NULL,
    Department VARCHAR(100) NOT NULL
);

-- Enrollments Table
CREATE TABLE Enrollments (
    EnrollmentID SERIAL PRIMARY KEY,
    StudentID INT REFERENCES Students(StudentID) ON DELETE CASCADE,
    CourseID INT REFERENCES Courses(CourseID) ON DELETE CASCADE,
    EnrollmentDate DATE NOT NULL
);

-- Grades Table
CREATE TABLE Grades (
    GradeID SERIAL PRIMARY KEY,
    StudentID INT REFERENCES Students(StudentID) ON DELETE CASCADE,
    CourseID INT REFERENCES Courses(CourseID) ON DELETE CASCADE,
    Grade VARCHAR(2) CHECK (Grade IN ('A', 'B', 'C', 'D', 'F')),
    Semester VARCHAR(10) NOT NULL
);

-- Departments Table
CREATE TABLE Departments (
    DepartmentID SERIAL PRIMARY KEY,
    DepartmentName VARCHAR(100) UNIQUE NOT NULL,
    Dean VARCHAR(100) NOT NULL
);

-- Attendance Table
CREATE TABLE Attendance (
    AttendanceID SERIAL PRIMARY KEY,
    StudentID INT REFERENCES Students(StudentID) ON DELETE CASCADE,
    CourseID INT REFERENCES Courses(CourseID) ON DELETE CASCADE,
    Date DATE NOT NULL,
    Status VARCHAR(10) CHECK (Status IN ('Present', 'Absent'))
);

INSERT INTO Departments (DepartmentName, Dean) VALUES
('Computer Science', 'Dr. Emily Carter'),
('Mathematics', 'Dr. Alan Turing'),
('Physics', 'Dr. Stephen Hawking'),
('Biology', 'Dr. Richard Dawkins'),
('Chemistry', 'Dr. Marie Curie'),
('Electrical Engineering', 'Dr. Nikola Tesla'),
('Mechanical Engineering', 'Dr. Henry Ford'),
('Civil Engineering', 'Dr. Gustave Eiffel'),
('Psychology', 'Dr. Sigmund Freud'),
('Economics', 'Dr. Adam Smith'),
('Political Science', 'Dr. Noam Chomsky'),
('History', 'Dr. Yuval Noah Harari'),
('Law', 'Dr. Ruth Bader Ginsburg'),
('Philosophy', 'Dr. Aristotle'),
('Medicine', 'Dr. Hippocrates');

INSERT INTO Professors (ProfessorName, Department) VALUES
('Dr. Alice Brown', 'Computer Science'),
('Dr. Robert Smith', 'Mathematics'),
('Dr. John Doe', 'Physics'),
('Dr. Sarah Johnson', 'Biology'),
('Dr. David Lee', 'Chemistry'),
('Dr. Laura Wilson', 'Electrical Engineering'),
('Dr. Kevin White', 'Mechanical Engineering'),
('Dr. Nancy Miller', 'Civil Engineering'),
('Dr. Thomas Garcia', 'Psychology'),
('Dr. Richard Martinez', 'Economics'),
('Dr. Maria Rodriguez', 'Political Science'),
('Dr. Charles Lopez', 'History'),
('Dr. Steven Harris', 'Law'),
('Dr. Emily Clark', 'Philosophy'),
('Dr. Daniel Young', 'Medicine');

INSERT INTO Courses (CourseName, Department, Credits) VALUES
('Database Systems', 'Computer Science', 3),
('Calculus', 'Mathematics', 4),
('Quantum Mechanics', 'Physics', 3),
('Genetics', 'Biology', 3),
('Organic Chemistry', 'Chemistry', 4),
('Circuit Analysis', 'Electrical Engineering', 3),
('Thermodynamics', 'Mechanical Engineering', 3),
('Structural Design', 'Civil Engineering', 4),
('Cognitive Psychology', 'Psychology', 3),
('Microeconomics', 'Economics', 3),
('Political Theory', 'Political Science', 3),
('World History', 'History', 4),
('Constitutional Law', 'Law', 3),
('Ethics', 'Philosophy', 3),
('Anatomy', 'Medicine', 4);

INSERT INTO Students (StudentName, DateOfBirth, Gender, EnrollmentDate, Major) VALUES
('John Doe', '2000-05-15', 'Male', '2023-08-01', 'Computer Science'),
('Jane Smith', '2001-07-22', 'Female', '2023-08-01', 'Mathematics'),
('Mike Johnson', '2002-09-10', 'Male', '2023-08-01', 'Physics'),
('Emily Davis', '2000-12-25', 'Female', '2023-08-01', 'Biology'),
('Chris Brown', '2001-06-30', 'Male', '2023-08-01', 'Chemistry'),
('Jessica Wilson', '2002-11-05', 'Female', '2023-08-01', 'Electrical Engineering'),
('David Miller', '2000-09-18', 'Male', '2023-08-01', 'Mechanical Engineering'),
('Sophia Martinez', '2001-03-22', 'Female', '2023-08-01', 'Civil Engineering'),
('Daniel Harris', '2002-08-14', 'Male', '2023-08-01', 'Psychology'),
('Olivia Clark', '2001-02-28', 'Female', '2023-08-01', 'Economics'),
('Matthew Rodriguez', '2000-10-12', 'Male', '2023-08-01', 'Political Science'),
('Isabella White', '2002-05-08', 'Female', '2023-08-01', 'History'),
('Lucas Allen', '2001-07-17', 'Male', '2023-08-01', 'Law'),
('Emma King', '2002-04-26', 'Female', '2023-08-01', 'Philosophy'),
('William Scott', '2000-11-15', 'Male', '2023-08-01', 'Medicine');

INSERT INTO Enrollments (StudentID, CourseID, EnrollmentDate) 
SELECT s.StudentID, c.CourseID, '2024-01-10' 
FROM Students s, Courses c 
WHERE s.StudentID = c.CourseID;

INSERT INTO Grades (StudentID, CourseID, Grade, Semester) 
SELECT StudentID, CourseID, 
       CASE 
           WHEN random() < 0.2 THEN 'A'
           WHEN random() < 0.4 THEN 'B'
           WHEN random() < 0.6 THEN 'C'
           WHEN random() < 0.8 THEN 'D'
           ELSE 'F'
       END, 
       'Fall 2023'
FROM Enrollments;

INSERT INTO Attendance (StudentID, CourseID, Date, Status) 
SELECT StudentID, CourseID, '2024-02-10', 
       CASE 
           WHEN random() < 0.8 THEN 'Present' 
           ELSE 'Absent' 
       END
FROM Enrollments;

-- 1. Retrieve the average grade for each course
SELECT g.CourseID, c.CourseName, AVG(
    CASE 
        WHEN g.Grade = 'A' THEN 4.0
        WHEN g.Grade = 'B' THEN 3.0
        WHEN g.Grade = 'C' THEN 2.0
        WHEN g.Grade = 'D' THEN 1.0
        WHEN g.Grade = 'F' THEN 0.0
    END
) AS AverageGPA
FROM Grades g
JOIN Courses c ON g.CourseID = c.CourseID
GROUP BY g.CourseID, c.CourseName;

-- 2. Find the top 5 students with the highest GPA
SELECT s.StudentID, s.StudentName, 
       AVG(
           CASE 
               WHEN g.Grade = 'A' THEN 4.0
               WHEN g.Grade = 'B' THEN 3.0
               WHEN g.Grade = 'C' THEN 2.0
               WHEN g.Grade = 'D' THEN 1.0
               WHEN g.Grade = 'F' THEN 0.0
           END
       ) AS GPA
FROM Grades g
JOIN Students s ON g.StudentID = s.StudentID
GROUP BY s.StudentID, s.StudentName
ORDER BY GPA DESC
LIMIT 5;

-- 3. Count the number of students enrolled in each major
SELECT Major, COUNT(*) AS StudentCount
FROM Students
GROUP BY Major;


-- 4. Identify the courses with the highest student enrollme
SELECT c.CourseID, c.CourseName, COUNT(e.StudentID) AS EnrollmentCount
FROM Enrollments e
JOIN Courses c ON e.CourseID = c.CourseID
GROUP BY c.CourseID, c.CourseName
ORDER BY EnrollmentCount DESC
LIMIT 5;

-- 5. Calculate the student retention rate
sql
Copy code
SELECT 
    (COUNT(DISTINCT StudentID) FILTER (WHERE EnrollmentDate <= '2023-08-01' AND EnrollmentDate > '2022-08-01') * 100.0 / 
     COUNT(DISTINCT StudentID) FILTER (WHERE EnrollmentDate <= '2022-08-01')) AS RetentionRate
FROM Students;


-- 6. Find the professors teaching the most courses
sql
Copy code
SELECT p.ProfessorID, p.ProfessorName, COUNT(c.CourseID) AS CourseCount
FROM Professors p
JOIN Courses c ON p.Department = c.Department
GROUP BY p.ProfessorID, p.ProfessorName
ORDER BY CourseCount DESC
LIMIT 5;


-- 7. List students who have failed more than one course
sql
Copy code
SELECT s.StudentID, s.StudentName, COUNT(*) AS FailCount
FROM Grades g
JOIN Students s ON g.StudentID = s.StudentID
WHERE g.Grade = 'F'
GROUP BY s.StudentID, s.StudentName
HAVING COUNT(*) > 1;


-- 8. Analyze semester-wise student performance trends
sql
Copy code
SELECT Semester, AVG(
    CASE 
        WHEN Grade = 'A' THEN 4.0
        WHEN Grade = 'B' THEN 3.0
        WHEN Grade = 'C' THEN 2.0
        WHEN Grade = 'D' THEN 1.0
        WHEN Grade = 'F' THEN 0.0
    END
) AS AverageGPA
FROM Grades
GROUP BY Semester
ORDER BY Semester;


-- 9. Calculate the percentage of students passing each course
sql
Copy code
SELECT g.CourseID, c.CourseName,
       (COUNT(*) FILTER (WHERE g.Grade IN ('A', 'B', 'C')) * 100.0 / COUNT(*)) AS PassPercentage
FROM Grades g
JOIN Courses c ON g.CourseID = c.CourseID
GROUP BY g.CourseID, c.CourseName;


-- 10. Find students who changed their major after enrollment
sql
Copy code
SELECT s.StudentID, s.StudentName
FROM Students s
WHERE EXISTS (
    SELECT 1
    FROM Students s2
    WHERE s.StudentID = s2.StudentID AND s.Major <> s2.Major
);


-- 11. Determine the course completion rate
sql
Copy code
SELECT g.CourseID, c.CourseName, 
       (COUNT(*) FILTER (WHERE g.Grade IS NOT NULL) * 100.0 / COUNT(*)) AS CompletionRate
FROM Enrollments e
LEFT JOIN Grades g ON e.StudentID = g.StudentID AND e.CourseID = g.CourseID
JOIN Courses c ON e.CourseID = c.CourseID
GROUP BY g.CourseID, c.CourseName;


-- 12. Identify professors whose students have the highest average grades
sql
Copy code
SELECT p.ProfessorID, p.ProfessorName, AVG(
    CASE 
        WHEN g.Grade = 'A' THEN 4.0
        WHEN g.Grade = 'B' THEN 3.0
        WHEN g.Grade = 'C' THEN 2.0
        WHEN g.Grade = 'D' THEN 1.0
        WHEN g.Grade = 'F' THEN 0.0
    END
) AS AvgGPA
FROM Professors p
JOIN Courses c ON p.Department = c.Department
JOIN Grades g ON c.CourseID = g.CourseID
GROUP BY p.ProfessorID, p.ProfessorName
ORDER BY AvgGPA DESC
LIMIT 5;


-- 13. Calculate the attendance rate for each student
sql
Copy code
SELECT a.StudentID, s.StudentName,
       (COUNT(*) FILTER (WHERE a.Status = 'Present') * 100.0 / COUNT(*)) AS AttendanceRate
FROM Attendance a
JOIN Students s ON a.StudentID = s.StudentID
GROUP BY a.StudentID, s.StudentName;


-- 14. Identify the most frequently skipped courses
sql
Copy code
SELECT a.CourseID, c.CourseName,
       (COUNT(*) FILTER (WHERE a.Status = 'Absent') * 100.0 / COUNT(*)) AS AbsenteeRate
FROM Attendance a
JOIN Courses c ON a.CourseID = c.CourseID
GROUP BY a.CourseID, c.CourseName
ORDER BY AbsenteeRate DESC
LIMIT 5;


-- 15. Find the department with the highest student enrollment
sql
Copy code
SELECT d.DepartmentID, d.DepartmentName, COUNT(s.StudentID) AS StudentCount
FROM Students s
JOIN Departments d ON s.Major = d.DepartmentName
GROUP BY d.DepartmentID, d.DepartmentName
ORDER BY StudentCount DESC
LIMIT 1;