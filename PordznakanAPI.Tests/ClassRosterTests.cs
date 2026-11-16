using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PordznakanAPI.Controllers;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Enums;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Tests
{
    public class EntityKeyTests
    {
        [Fact]
        public void Guid_is_read_as_internal_id()
        {
            var guid = Guid.NewGuid();

            Assert.True(EntityKey.TryParse(guid.ToString(), out var key));
            Assert.Equal(guid, key.InternalId);
            Assert.Null(key.KtakId);
        }

        [Fact]
        public void Integer_is_read_as_ktak_id()
        {
            Assert.True(EntityKey.TryParse(" 2302379 ", out var key));
            Assert.Equal(2302379, key.KtakId);
            Assert.Null(key.InternalId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        [InlineData("12.5")]
        public void Anything_else_is_invalid(string? raw)
        {
            Assert.False(EntityKey.TryParse(raw, out _));
        }
    }

    public class ClassRosterServiceTests
    {
        private const int SchoolA = 373;
        private const int SchoolB = 400;

        private static AppDbContext NewContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static Classroom Class(int school, string ktakId, string grade, string classifier) => new()
        {
            Id = Guid.NewGuid(),
            KtakSchoolId = school,
            KtakClassroomId = ktakId,
            Grade = grade,
            Classifier = classifier,
            ClassName = grade + classifier
        };

        private static Pupil PupilIn(Classroom c, int ktakId, string last, string first = "X") => new()
        {
            Id = Guid.NewGuid(),
            KtakPupilId = ktakId,
            KtakSchoolId = c.KtakSchoolId,
            ClassroomId = c.KtakClassroomId,
            ClassroomInternalId = c.Id,
            LastName = last,
            FirstName = first
        };

        private static Teacher NewTeacher(int ktakId, int school) => new()
        {
            Id = Guid.NewGuid(),
            KtakTeacherId = ktakId,
            KtakSchoolId = school,
            FirstName = "Ann",
            LastName = "Teacher",
            MainSubject = "Math"
        };

        private static TeacherSubject Subject(Teacher t, int subjectId, string name, EGrade grade, ESubGrade sub) => new()
        {
            Id = Guid.NewGuid(),
            TeacherId = t.Id,
            SubjectId = subjectId,
            Name = name,
            Grade = grade,
            SubGrade = sub
        };

        // ---- Teacher classes ----

        [Fact]
        public async Task Teacher_classes_returns_matching_classes_with_their_pupils()
        {
            using var db = NewContext();
            var teacher = NewTeacher(762, SchoolA);
            var c5a = Class(SchoolA, "1", "5", "ա");
            var c5b = Class(SchoolA, "2", "5", "բ");
            var c6a = Class(SchoolA, "3", "6", "ա");
            var otherSchool5a = Class(SchoolB, "9", "5", "ա");   // same grade/letter, different school
            db.Teachers.Add(teacher);
            db.TeacherSubjects.AddRange(
                Subject(teacher, 3, "Technology", EGrade.G5, ESubGrade.Sg1),
                Subject(teacher, 3, "Technology", EGrade.G6, ESubGrade.Sg1));
            db.Classrooms.AddRange(c5a, c5b, c6a, otherSchool5a);
            db.Pupils.AddRange(
                PupilIn(c5a, 1, "Beta"), PupilIn(c5a, 2, "Alpha"),
                PupilIn(c6a, 3, "Gamma"),
                PupilIn(c5b, 4, "NotMine"),
                PupilIn(otherSchool5a, 5, "OtherSchool"));
            await db.SaveChangesAsync();

            var result = await new ClassRosterService(db).GetTeacherClassesAsync("762");

            Assert.Equal(RosterStatus.Ok, result.Status);
            var response = result.Value!;
            Assert.Equal(762, response.Teacher.KtakTeacherId);
            Assert.Equal(new[] { "5ա", "6ա" }, response.Classes.Select(c => c.ClassName));
            Assert.Equal(new[] { "Alpha", "Beta" }, response.Classes[0].Pupils.Select(p => p.LastName));
            Assert.Equal(2, response.Classes[0].PupilCount);
            Assert.Equal(new[] { "Gamma" }, response.Classes[1].Pupils.Select(p => p.LastName));
            Assert.Equal(new[] { "Technology" }, response.Classes[0].Subjects);
        }

        [Fact]
        public async Task Teacher_can_be_found_by_internal_id_or_ktak_id()
        {
            using var db = NewContext();
            var teacher = NewTeacher(762, SchoolA);
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync();
            var service = new ClassRosterService(db);

            var byKtak = await service.GetTeacherClassesAsync("762");
            var byGuid = await service.GetTeacherClassesAsync(teacher.Id.ToString());

            Assert.Equal(RosterStatus.Ok, byKtak.Status);
            Assert.Equal(RosterStatus.Ok, byGuid.Status);
            Assert.Equal(teacher.Id, byKtak.Value!.Teacher.Id);
            Assert.Equal(teacher.Id, byGuid.Value!.Teacher.Id);
        }

        [Fact]
        public async Task Upper_case_classifier_from_emis_still_matches_the_class()
        {
            using var db = NewContext();
            var teacher = NewTeacher(1, SchoolA);
            var upperRo = Class(SchoolA, "1", "7", "Ռ");   // EMIS sends the upper-case letter
            db.Teachers.Add(teacher);
            db.TeacherSubjects.Add(Subject(teacher, 1, "Art", EGrade.G7, ESubGrade.Sg18));
            db.Classrooms.Add(upperRo);
            await db.SaveChangesAsync();

            var result = await new ClassRosterService(db).GetTeacherClassesAsync("1");

            Assert.Single(result.Value!.Classes);
        }

        [Fact]
        public async Task Teacher_without_subjects_has_no_classes()
        {
            using var db = NewContext();
            db.Teachers.Add(NewTeacher(1, SchoolA));
            db.Classrooms.Add(Class(SchoolA, "1", "5", "ա"));
            await db.SaveChangesAsync();

            var result = await new ClassRosterService(db).GetTeacherClassesAsync("1");

            Assert.Equal(RosterStatus.Ok, result.Status);
            Assert.Empty(result.Value!.Classes);
        }

        [Fact]
        public async Task Unknown_teacher_is_not_found_by_either_id_type()
        {
            using var db = NewContext();
            var service = new ClassRosterService(db);

            Assert.Equal(RosterStatus.NotFound, (await service.GetTeacherClassesAsync("999")).Status);
            Assert.Equal(RosterStatus.NotFound, (await service.GetTeacherClassesAsync(Guid.NewGuid().ToString())).Status);
            Assert.Equal(RosterStatus.NotFound, (await service.GetTeacherSubjectsAsync("999")).Status);
        }

        [Fact]
        public async Task Malformed_id_is_invalid()
        {
            using var db = NewContext();
            var service = new ClassRosterService(db);

            Assert.Equal(RosterStatus.InvalidId, (await service.GetTeacherClassesAsync("abc")).Status);
            Assert.Equal(RosterStatus.InvalidId, (await service.GetTeacherSubjectsAsync("0")).Status);
            Assert.Equal(RosterStatus.InvalidId, (await service.GetClassmatesAsync("-1")).Status);
        }

        // ---- Teacher subjects ----

        [Fact]
        public async Task Teacher_subjects_are_grouped_by_subject_with_their_grades()
        {
            using var db = NewContext();
            var teacher = NewTeacher(5, SchoolA);
            db.Teachers.Add(teacher);
            db.TeacherSubjects.AddRange(
                Subject(teacher, 39, "Mother tongue", EGrade.G6, ESubGrade.Sg1),
                Subject(teacher, 39, "Mother tongue", EGrade.G5, ESubGrade.Sg1),
                Subject(teacher, 44, "Armenian", EGrade.G7, ESubGrade.Sg1),
                Subject(NewTeacher(6, SchoolA), 1, "Other teacher's subject", EGrade.G1, ESubGrade.Sg1));
            await db.SaveChangesAsync();

            var result = await new ClassRosterService(db).GetTeacherSubjectsAsync("5");

            Assert.Equal(RosterStatus.Ok, result.Status);
            var subjects = result.Value!.Subjects;
            Assert.Equal(new[] { 44, 39 }, subjects.Select(s => s.SubjectId));   // ordered by name
            var tongue = subjects.Single(s => s.SubjectId == 39);
            Assert.Equal(new[] { EGrade.G5, EGrade.G6 }, tongue.Grades.Select(g => g.Grade));
        }

        // ---- Classmates ----

        [Fact]
        public async Task Classmates_exclude_the_pupil_and_other_classes()
        {
            using var db = NewContext();
            var c = Class(SchoolA, "1", "5", "ա");
            var other = Class(SchoolA, "2", "5", "բ");
            var me = PupilIn(c, 100, "Me");
            db.Classrooms.AddRange(c, other);
            db.Pupils.AddRange(me, PupilIn(c, 101, "Zed"), PupilIn(c, 102, "Abe"), PupilIn(other, 103, "Stranger"));
            await db.SaveChangesAsync();

            var result = await new ClassRosterService(db).GetClassmatesAsync("100");

            Assert.Equal(RosterStatus.Ok, result.Status);
            var response = result.Value!;
            Assert.Equal(100, response.Pupil.KtakPupilId);
            Assert.Equal(c.Id, response.Class!.Id);
            Assert.Equal(new[] { "Abe", "Zed" }, response.Classmates.Select(p => p.LastName));
            Assert.Equal(2, response.ClassmateCount);
        }

        [Fact]
        public async Task Pupil_can_be_found_by_internal_id_or_ktak_id()
        {
            using var db = NewContext();
            var c = Class(SchoolA, "1", "5", "ա");
            var me = PupilIn(c, 100, "Me");
            db.Classrooms.Add(c);
            db.Pupils.AddRange(me, PupilIn(c, 101, "Mate"));
            await db.SaveChangesAsync();
            var service = new ClassRosterService(db);

            var byKtak = await service.GetClassmatesAsync("100");
            var byGuid = await service.GetClassmatesAsync(me.Id.ToString());

            Assert.Equal(me.Id, byKtak.Value!.Pupil.Id);
            Assert.Equal(me.Id, byGuid.Value!.Pupil.Id);
            Assert.Single(byGuid.Value.Classmates);
        }

        [Fact]
        public async Task Classmates_fall_back_to_the_emis_classroom_id_when_the_link_is_missing()
        {
            using var db = NewContext();
            var c = Class(SchoolA, "7", "5", "ա");
            var me = PupilIn(c, 100, "Me");
            me.ClassroomInternalId = null;
            var mate = PupilIn(c, 101, "Mate");
            mate.ClassroomInternalId = null;
            var otherSchoolSameClassId = PupilIn(Class(SchoolB, "7", "5", "ա"), 102, "Elsewhere");
            otherSchoolSameClassId.ClassroomInternalId = null;
            db.Classrooms.Add(c);
            db.Pupils.AddRange(me, mate, otherSchoolSameClassId);
            await db.SaveChangesAsync();

            var result = await new ClassRosterService(db).GetClassmatesAsync("100");

            Assert.Equal(new[] { "Mate" }, result.Value!.Classmates.Select(p => p.LastName));
            Assert.Equal(c.Id, result.Value.Class!.Id);
        }

        [Fact]
        public async Task Unknown_pupil_is_not_found_by_either_id_type()
        {
            using var db = NewContext();
            var service = new ClassRosterService(db);

            Assert.Equal(RosterStatus.NotFound, (await service.GetClassmatesAsync("999")).Status);
            Assert.Equal(RosterStatus.NotFound, (await service.GetClassmatesAsync(Guid.NewGuid().ToString())).Status);
        }
    }

    public class ClassRosterControllerTests
    {
        private static ClassRosterController NewController(out AppDbContext db)
        {
            db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            return new ClassRosterController(new ClassRosterService(db));
        }

        [Fact]
        public async Task Missing_teacher_gives_404()
        {
            var controller = NewController(out var db);
            using var _ = db;

            Assert.IsType<NotFoundObjectResult>(await controller.GetTeacherClasses("123", default));
            Assert.IsType<NotFoundObjectResult>(await controller.GetTeacherSubjects("123", default));
        }

        [Fact]
        public async Task Missing_pupil_gives_404()
        {
            var controller = NewController(out var db);
            using var _ = db;

            Assert.IsType<NotFoundObjectResult>(await controller.GetClassmates("123", default));
        }

        [Fact]
        public async Task Malformed_id_gives_400()
        {
            var controller = NewController(out var db);
            using var _ = db;

            Assert.IsType<BadRequestObjectResult>(await controller.GetTeacherClasses("not-an-id", default));
            Assert.IsType<BadRequestObjectResult>(await controller.GetClassmates("0", default));
        }

        [Fact]
        public async Task Existing_teacher_gives_200_with_the_response_body()
        {
            var controller = NewController(out var db);
            using var _ = db;
            db.Teachers.Add(new Teacher { Id = Guid.NewGuid(), KtakTeacherId = 7, KtakSchoolId = 1 });
            await db.SaveChangesAsync();

            var ok = Assert.IsType<OkObjectResult>(await controller.GetTeacherSubjects("7", default));
            Assert.IsType<TeacherSubjectsResponse>(ok.Value);
        }
    }
}

namespace PordznakanAPI.Tests
{
    public class ClassRosterJsonTests
    {
        [Fact]
        public void Class_fields_are_written_before_subjects_and_pupils()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                new TeacherClassDto { ClassName = "5ա", Grade = "5", Classifier = "ա" },
                new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

            Assert.True(json.IndexOf("\"className\"") < json.IndexOf("\"subjects\""), json);
            Assert.True(json.IndexOf("\"id\"") < json.IndexOf("\"pupils\""), json);
        }
    }
}
