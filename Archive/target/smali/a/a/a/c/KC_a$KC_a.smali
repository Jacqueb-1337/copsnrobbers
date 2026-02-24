.class final La/a/a/c/KC_a$KC_a;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Landroid/os/Parcelable$Creator;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/c/KC_a;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_a"
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "<T:",
        "Ljava/lang/Object;",
        ">",
        "Ljava/lang/Object;",
        "Landroid/os/Parcelable$Creator",
        "<TT;>;"
    }
.end annotation


# instance fields
.field private a:La/a/a/c/KC_b;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/c/KC_b",
            "<TT;>;"
        }
    .end annotation
.end field


# direct methods
.method public constructor <init>(La/a/a/c/KC_b;)V
    .locals 0
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "La/a/a/c/KC_b",
            "<TT;>;)V"
        }
    .end annotation

    .prologue
    .line 45
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 46
    iput-object p1, p0, La/a/a/c/KC_a$KC_a;->a:La/a/a/c/KC_b;

    .line 47
    return-void
.end method


# virtual methods
.method public final createFromParcel(Landroid/os/Parcel;)Ljava/lang/Object;
    .locals 2
    .param p1, "source"    # Landroid/os/Parcel;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Landroid/os/Parcel;",
            ")TT;"
        }
    .end annotation

    .prologue
    .line 51
    .local p0, "this":La/a/a/c/KC_a$KC_a;, "La/a/a/c/KC_a$KC_a<TT;>;"
    iget-object v0, p0, La/a/a/c/KC_a$KC_a;->a:La/a/a/c/KC_b;

    const/4 v1, 0x0

    invoke-interface {v0, p1, v1}, La/a/a/c/KC_b;->a(Landroid/os/Parcel;Ljava/lang/ClassLoader;)Ljava/lang/Object;

    move-result-object v0

    return-object v0
.end method

.method public final newArray(I)[Ljava/lang/Object;
    .locals 1
    .param p1, "size"    # I
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(I)[TT;"
        }
    .end annotation

    .prologue
    .line 56
    .local p0, "this":La/a/a/c/KC_a$KC_a;, "La/a/a/c/KC_a$KC_a<TT;>;"
    iget-object v0, p0, La/a/a/c/KC_a$KC_a;->a:La/a/a/c/KC_b;

    invoke-interface {v0, p1}, La/a/a/c/KC_b;->a(I)[Ljava/lang/Object;

    move-result-object v0

    return-object v0
.end method
